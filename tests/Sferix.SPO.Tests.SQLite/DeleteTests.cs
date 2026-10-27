using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.ObjectSpace;
using Sferix.SPO.Provider.SQLite;
using Xunit;

namespace Sferix.SPO.Tests.SQLite;

public sealed class DeleteTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"spo-del-{Guid.NewGuid():N}.db");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero));

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm" })
            File.Delete(file);
    }

    private ISpoObjectSpace Open() =>
        new SpoModelBuilder().AddModule<SampleModule>().UseTimeProvider(_time).Build(new SqliteProvider()).OpenObjectSpace($"Data Source={_path}");

    private static Person NewPerson(string name = "Ann") => new() { Name = name, Balance = 1.5m, Joined = DateTimeOffset.UnixEpoch };

    private (bool IsDeleted, DateTime? DeletedAt, DateTime ModifiedAt, long Count) RawRow(Guid id)
    {
        using var raw = new SqliteConnection($"Data Source={_path}");
        raw.Open();
        using var command = raw.CreateCommand();
        command.CommandText = "SELECT IsDeleted, DeletedAt, ModifiedAt, (SELECT count(*) FROM Person) FROM Person WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id.ToByteArray(bigEndian: true));
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        return (reader.GetInt64(0) != 0, reader.IsDBNull(1) ? null : new DateTime(reader.GetInt64(1), DateTimeKind.Utc),
            new DateTime(reader.GetInt64(2), DateTimeKind.Utc), reader.GetInt64(3));
    }

    private Guid SavePerson()
    {
        using var space = Open();
        var id = space.Add(NewPerson()).Id;
        space.SaveChanges();
        return id;
    }

    [Fact]
    public void DeleteAndSaveHidesTheRowButKeepsItWithFlagsSet()
    {
        var id = SavePerson();
        _time.Now = _time.Now.AddHours(2);
        using (var space = Open())
        {
            var person = space.Get<Person>(id)!;
            space.Delete(person);
            Assert.Null(space.Get<Person>(id)); // hidden before the save
            space.SaveChanges();
            Assert.True(person.IsDeleted);
            Assert.Equal(_time.Now.UtcDateTime, person.DeletedAt);
            Assert.Null(space.Get<Person>(id)); // and after
        }

        var row = RawRow(id);
        Assert.Equal((true, _time.Now.UtcDateTime, _time.Now.UtcDateTime, 1L), (row.IsDeleted, row.DeletedAt, row.ModifiedAt, row.Count));
        using var reopened = Open();
        Assert.Null(reopened.Get<Person>(id));
    }

    [Fact]
    public void DeleteOfAnAddedEntityInsertsNothing()
    {
        using var space = Open();
        var person = space.Add(NewPerson());
        space.Delete(person);
        space.SaveChanges();
        Assert.Null(space.Get<Person>(person.Id));
        using var raw = new SqliteConnection($"Data Source={_path}");
        raw.Open();
        using var command = raw.CreateCommand();
        command.CommandText = "SELECT count(*) FROM Person";
        Assert.Equal(0L, command.ExecuteScalar());
    }

    [Fact]
    public void DeleteOfAnUntrackedEntityFailsWithSpo1012()
    {
        var id = SavePerson();
        using var first = Open();
        var loaded = first.Get<Person>(id)!;
        using var second = Open();
        Assert.Equal("SPO1012", Assert.Throws<SpoObjectSpaceException>(() => second.Delete(loaded)).Code);
        Assert.Equal("SPO1012", Assert.Throws<SpoObjectSpaceException>(() => second.Delete(NewPerson())).Code);
        Assert.Equal("SPO1012", Assert.Throws<SpoObjectSpaceException>(() => second.Delete(new Person { Id = id })).Code);
    }

    [Fact]
    public void FailedSaveKeepsTheEntityPendingDeletionAndRestoresTheFlags()
    {
        var id = SavePerson();
        _time.Now = _time.Now.AddHours(1);
        using var space = Open();
        var person = space.Get<Person>(id)!;
        var modified = person.ModifiedAt;
        space.Delete(person);
        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "ALTER TABLE Person RENAME TO PersonMoved";
            command.ExecuteNonQuery();
        }

        Assert.ThrowsAny<Exception>(() => space.SaveChanges());
        Assert.False(person.IsDeleted);
        Assert.Null(person.DeletedAt);
        Assert.Equal(modified, person.ModifiedAt);
        Assert.Null(space.Get<Person>(id)); // still pending deletion

        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "ALTER TABLE PersonMoved RENAME TO Person";
            command.ExecuteNonQuery();
        }

        space.SaveChanges();
        Assert.True(RawRow(id).IsDeleted);
    }

    [Fact]
    public void DeletingTwiceIsHarmless()
    {
        var id = SavePerson();
        using var space = Open();
        var person = space.Get<Person>(id)!;
        space.Delete(person);
        space.Delete(person);
        space.SaveChanges();
        Assert.True(RawRow(id).IsDeleted);
    }

    [Fact]
    public async Task AsyncDeleteAndGet()
    {
        var id = SavePerson();
        await using (var space = Open())
        {
            var person = (await space.GetAsync<Person>(id, CancellationToken.None))!;
            space.Delete(person);
            await space.SaveChangesAsync(CancellationToken.None);
        }
        await using var reopened = Open();
        Assert.Null(await reopened.GetAsync<Person>(id, CancellationToken.None));
    }

    [Fact]
    public void ScaleChangeOfADecimalIsSaved()
    {
        var id = SavePerson();
        using (var space = Open())
        {
            var person = space.Get<Person>(id)!;
            Assert.Equal(1, (decimal.GetBits(person.Balance)[3] >> 16) & 0xFF);
            person.Balance = 1.50m;
            space.SaveChanges();
        }

        using var check = Open();
        Assert.Equal(2, (decimal.GetBits(check.Get<Person>(id)!.Balance)[3] >> 16) & 0xFF);
    }

    [Fact]
    public void ByteArrayWithEqualContentIsNotAChange()
    {
        Guid id;
        using (var space = Open())
        {
            var person = NewPerson();
            person.Photo = [1, 2];
            id = space.Add(person).Id;
            space.SaveChanges();
        }

        _time.Now = _time.Now.AddHours(1);
        using var again = Open();
        var loaded = again.Get<Person>(id)!;
        var modified = loaded.ModifiedAt;
        loaded.Photo = [1, 2];
        again.SaveChanges();
        Assert.Equal(modified, loaded.ModifiedAt);
        loaded.Photo = [1, 3];
        again.SaveChanges();
        Assert.Equal(_time.Now.UtcDateTime, loaded.ModifiedAt);
    }
}
