using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Entities;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Modules;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.ObjectSpace;
using Sferix.SPO.Provider.SQLite;
using Xunit;

namespace Sferix.SPO.Tests.SQLite;

public enum Tier : byte { Basic = 1, Gold = 2 }

[SpoEntity("6f1c0b5e-2d4a-4c8e-9a1b-0e7d3f5a8c11")]
public partial class Person : SPOBaseObject
{
    [SpoField(1)] public string Name { get; set; } = "";
    [SpoField(2)] public int? Age { get; set; }
    [SpoField(3)] public decimal Balance { get; set; }
    [SpoField(4)] public Tier Level { get; set; }
    [SpoField(5)] public byte[]? Photo { get; set; }
    [SpoField(6)] public DateTimeOffset Joined { get; set; }
}

[SpoEntity("0a7b9c1d-3e5f-4a6b-8c2d-1f3e5a7b9c22")]
public partial class Gadget : SPOBaseObject
{
    private ISpoObjectSpace? _spy;
    private string _label = "";

    [SpoField(1)]
    public string Label
    {
        get
        {
            // Used by the re-entrancy test: reading the property calls back into the ObjectSpace.
            _spy?.Get<Gadget>(Guid.NewGuid());
            return _label;
        }
        set => _label = value;
    }

    public void Spy(ISpoObjectSpace space) => _spy = space;
    public void StopSpying() => _spy = null;
}

public sealed class SampleModule : ISpoModule
{
    public IEnumerable<EntityMetadata> Entities => [Person.SpoMetadata, Gadget.SpoMetadata];
}

public sealed class NotAnEntity : SPOBaseObject;

public sealed class FakeTime(DateTimeOffset start) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = start;
    public override DateTimeOffset GetUtcNow() => Now;
}

public sealed class ObjectSpaceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"spo-os-{Guid.NewGuid():N}.db");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero));

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm" })
            File.Delete(file);
    }

    private SpoModel Model() => new SpoModelBuilder().AddModule<SampleModule>().UseTimeProvider(_time).Build(new SqliteProvider());
    private ISpoObjectSpace Open() => Model().OpenObjectSpace($"Data Source={_path}");

    private static Person NewPerson(string name = "Ann") => new()
    {
        Name = name, Age = 30, Balance = 1.50m, Level = Tier.Gold, Photo = [1, 2, 3],
        Joined = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)),
    };

    private static long Count(string path, string table)
    {
        using var raw = new SqliteConnection($"Data Source={path}");
        raw.Open();
        using var command = raw.CreateCommand();
        command.CommandText = $"SELECT count(*) FROM \"{table}\"";
        return (long)command.ExecuteScalar()!;
    }

    [Fact]
    public void AddedEntityGetsUuidV7FromTimeProviderAndIsReadByANewObjectSpace()
    {
        Guid id;
        using (var space = Open())
        {
            var person = space.Add(NewPerson());
            Assert.NotEqual(Guid.Empty, person.Id);
            Assert.Equal(7, (person.Id.ToString("N")[12] - '0'));
            space.SaveChanges();
            id = person.Id;
            Assert.Equal(_time.Now.UtcDateTime, person.CreatedAt);
            Assert.Equal(person.CreatedAt, person.ModifiedAt);
        }

        using var second = Open();
        var read = second.Get<Person>(id)!;
        Assert.Equal(("Ann", 30, 1.50m, Tier.Gold), (read.Name, read.Age, read.Balance, read.Level));
        Assert.Equal([1, 2, 3], read.Photo);
        Assert.Equal(TimeSpan.FromHours(2), read.Joined.Offset);
        Assert.Equal(_time.Now.UtcDateTime, read.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, read.CreatedAt.Kind);
        Assert.Null(second.Get<Person>(Guid.NewGuid()));
    }

    [Fact]
    public void ExistingIdIsKept()
    {
        var id = Guid.CreateVersion7();
        using var space = Open();
        space.Add(NewPerson()).Id = id;
        space.SaveChanges();
        Assert.NotNull(Open().Get<Person>(id));
    }

    [Fact]
    public void ChangedEntityIsUpdatedWithNewModifiedAtOnly()
    {
        Guid id;
        using (var space = Open())
        {
            id = space.Add(NewPerson()).Id;
            space.SaveChanges();
        }

        _time.Now = _time.Now.AddHours(1);
        using (var space = Open())
        {
            var person = space.Get<Person>(id)!;
            person.Name = "Anna";
            person.Age = null;
            space.SaveChanges();
            Assert.Equal(_time.Now.UtcDateTime, person.ModifiedAt);
        }

        using var check = Open();
        var read = check.Get<Person>(id)!;
        Assert.Equal("Anna", read.Name);
        Assert.Null(read.Age);
        Assert.Equal(1.50m, read.Balance);
        Assert.Equal(_time.Now.AddHours(-1).UtcDateTime, read.CreatedAt);
        Assert.Equal(_time.Now.UtcDateTime, read.ModifiedAt);
    }

    [Fact]
    public void UnchangedEntityIsNotWritten()
    {
        Guid id;
        using (var space = Open())
        {
            id = space.Add(NewPerson()).Id;
            space.SaveChanges();
        }

        _time.Now = _time.Now.AddHours(1);
        using (var space = Open())
        {
            var person = space.Get<Person>(id)!;
            person.Photo = [1, 2, 3]; // equal content, new array: not a change
            space.SaveChanges();
            Assert.Equal(_time.Now.AddHours(-1).UtcDateTime, person.ModifiedAt);
        }
    }

    [Fact]
    public void SecondSaveAfterSuccessDoesNotWriteAgain()
    {
        using var space = Open();
        var person = space.Add(NewPerson());
        space.SaveChanges();
        var modified = person.ModifiedAt;
        _time.Now = _time.Now.AddHours(1);
        space.SaveChanges();
        Assert.Equal(modified, person.ModifiedAt);
        Assert.Equal(1, Count(_path, "Person"));
    }

    [Fact]
    public void IdentityMapReturnsTheSameInstance()
    {
        using var space = Open();
        var added = space.Add(NewPerson());
        Assert.Same(added, space.Get<Person>(added.Id));
        space.SaveChanges();
        Assert.Same(added, space.Get<Person>(added.Id));

        using var other = Open();
        var first = other.Get<Person>(added.Id);
        Assert.Same(first, other.Get<Person>(added.Id));
        Assert.NotSame(added, first);
    }

    [Fact]
    public void DifferentInstanceWithTrackedKeyIsRejected()
    {
        using var space = Open();
        var person = space.Add(NewPerson());
        Assert.Same(person, space.Add(person));
        Assert.Throws<ArgumentException>(() => space.Add(new Person { Id = person.Id, Name = "Other" }));
    }

    [Fact]
    public void TypeOutsideTheModelIsRejected()
    {
        using var space = Open();
        var ex = Assert.Throws<SpoObjectSpaceException>(() => space.Add(new NotAnEntity()));
        Assert.Equal("SPO1008", ex.Code);
        Assert.Equal("SPO1008", Assert.Throws<SpoObjectSpaceException>(() => space.Get<NotAnEntity>(Guid.NewGuid())).Code);
    }

    [Fact]
    public void FailedSaveRollsBackKeepsPendingChangesAndSucceedsAfterTheFix()
    {
        Guid existing;
        using (var setup = Open())
        {
            existing = setup.Add(NewPerson("Old")).Id;
            setup.SaveChanges();
        }

        _time.Now = _time.Now.AddHours(1);
        using var space = Open();
        var loaded = space.Get<Person>(existing)!;
        var loadedModified = loaded.ModifiedAt;
        loaded.Name = "Changed";
        var added = space.Add(NewPerson("New"));
        added.CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Break the save: remove the table the insert needs, so the second statement fails after the first succeeded.
        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "ALTER TABLE Person RENAME TO PersonMoved";
            command.ExecuteNonQuery();
        }

        Assert.ThrowsAny<Exception>(() => space.SaveChanges());
        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), added.CreatedAt); // restored
        Assert.Equal(default, added.ModifiedAt);
        Assert.Equal(loadedModified, loaded.ModifiedAt);
        Assert.Equal("Changed", loaded.Name);

        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "ALTER TABLE PersonMoved RENAME TO Person";
            command.ExecuteNonQuery();
        }

        space.SaveChanges();
        Assert.Equal(2, Count(_path, "Person"));
        using var check = Open();
        Assert.Equal("Changed", check.Get<Person>(existing)!.Name);
        Assert.Equal(_time.Now.UtcDateTime, check.Get<Person>(existing)!.ModifiedAt);
    }

    [Fact]
    public void FailedInsertLeavesNoRowsAndCreatedAtRestored()
    {
        using var space = Open();
        var first = space.Add(NewPerson("A"));
        var second = space.Add(NewPerson("B"));
        second.Id = first.Id; // not tracked under the new key; the database rejects the duplicate key
        Assert.ThrowsAny<Exception>(() => space.SaveChanges());
        Assert.Equal(0, Count(_path, "Person"));
        Assert.Equal(default, first.CreatedAt);
        Assert.Equal(default, second.ModifiedAt);
    }

    [Fact]
    public void UpdateOfDeletedRowFailsWithSpo1009AndRollsBack()
    {
        Guid id;
        using (var setup = Open())
        {
            id = setup.Add(NewPerson()).Id;
            setup.SaveChanges();
        }

        using var space = Open();
        var person = space.Get<Person>(id)!;
        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "DELETE FROM Person";
            command.ExecuteNonQuery();
        }

        person.Name = "Gone";
        var modified = person.ModifiedAt;
        var ex = Assert.Throws<SpoObjectSpaceException>(() => space.SaveChanges());
        Assert.Equal("SPO1009", ex.Code);
        Assert.Contains("Person", ex.Message);
        Assert.Equal(modified, person.ModifiedAt);
        Assert.Equal("Gone", person.Name);
    }

    [Fact]
    public void InsertBeforeAFailingUpdateIsRolledBack()
    {
        Guid id;
        using (var setup = Open())
        {
            id = setup.Add(NewPerson()).Id;
            setup.SaveChanges();
        }

        using var space = Open();
        var loaded = space.Get<Person>(id)!;
        var added = space.Add(NewPerson("New"));
        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "DELETE FROM Person";
            command.ExecuteNonQuery();
        }

        loaded.Name = "Gone";
        Assert.Equal("SPO1009", Assert.Throws<SpoObjectSpaceException>(() => space.SaveChanges()).Code);
        Assert.Equal(0, Count(_path, "Person"));
        Assert.Equal(default, added.CreatedAt);
    }

    [Fact]
    public void CallDuringSaveFromAGetterIsReportedAsConcurrentUseAndTheSpaceStaysUsable()
    {
        using var space = Open();
        var gadget = space.Add(new Gadget { Label = "g" });
        gadget.Spy(space);
        var ex = Assert.Throws<SpoObjectSpaceException>(() => space.SaveChanges());
        Assert.Equal("SPO1010", ex.Code);
        Assert.Equal(0, Count(_path, "Gadget"));

        gadget.StopSpying();
        space.SaveChanges();
        Assert.Equal(1, Count(_path, "Gadget"));
    }

    [Fact]
    public async Task ConcurrentCallFromAnotherThreadIsReported()
    {
        using var space = Open();
        var gadget = space.Add(new Gadget { Label = "g" });
        var inside = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        gadget.Spy(new BlockingSpace(inside, release));
        var save = Task.Run(() => space.SaveChanges());
        Assert.True(inside.Wait(TimeSpan.FromSeconds(10)));
        try
        {
            Assert.Equal("SPO1010", Assert.Throws<SpoObjectSpaceException>(() => space.Get<Person>(Guid.NewGuid())).Code);
        }
        finally
        {
            release.Set();
        }
        await save;
        Assert.Equal(1, Count(_path, "Gadget"));
    }

    private sealed class BlockingSpace(ManualResetEventSlim inside, ManualResetEventSlim release) : ISpoObjectSpace
    {
        public T? Get<T>(Guid id) where T : SPOBaseObject
        {
            inside.Set();
            release.Wait(TimeSpan.FromSeconds(10));
            return null;
        }

        public T Add<T>(T entity) where T : SPOBaseObject => throw new NotSupportedException();
        public Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken) where T : SPOBaseObject => throw new NotSupportedException();
        public void SaveChanges() => throw new NotSupportedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public void CallsAfterDisposeThrowObjectDisposedException()
    {
        var space = Open();
        space.Dispose();
        space.Dispose();
        Assert.Throws<ObjectDisposedException>(() => space.Add(NewPerson()));
        Assert.Throws<ObjectDisposedException>(() => space.Get<Person>(Guid.NewGuid()));
        Assert.Throws<ObjectDisposedException>(() => space.SaveChanges());
    }

    [Fact]
    public async Task AsyncPathsAddSaveGetAndUpdate()
    {
        await using var space = Open();
        var person = space.Add(NewPerson());
        await space.SaveChangesAsync(CancellationToken.None);
        await using var other = Open();
        var read = (await other.GetAsync<Person>(person.Id, CancellationToken.None))!;
        read.Name = "Async";
        await other.SaveChangesAsync(CancellationToken.None);
        await using var third = Open();
        Assert.Equal("Async", (await third.GetAsync<Person>(person.Id, CancellationToken.None))!.Name);

        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => space.SaveChangesAsync(cancelled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => space.GetAsync<Person>(person.Id, cancelled.Token));
    }

    [Fact]
    public void HostileTextIsStoredLiterally()
    {
        const string text = "'); DROP TABLE Person; -- % _";
        Guid id;
        using (var space = Open())
        {
            id = space.Add(NewPerson(text)).Id;
            space.SaveChanges();
        }
        using var check = Open();
        Assert.Equal(text, check.Get<Person>(id)!.Name);
        Assert.Equal(1, Count(_path, "Person"));
    }

    [Fact]
    public void ReopeningWithMismatchingSchemaIsRejected()
    {
        using (Open()) { }
        using (var raw = new SqliteConnection($"Data Source={_path}"))
        {
            raw.Open();
            using var command = raw.CreateCommand();
            command.CommandText = "ALTER TABLE Person DROP COLUMN Photo";
            command.ExecuteNonQuery();
        }
        Assert.ThrowsAny<Exception>(() => Open());
    }

    [Fact]
    public void BuildReportsSchemaErrorsAtActivation()
    {
        var ex = Assert.Throws<SpoModelException>(() => new SpoModelBuilder().AddModule<DuplicateTableModule>().Build(new SqliteProvider()));
        Assert.Equal("SPO1005", ex.Code);
    }

    private sealed class DuplicateTableModule : ISpoModule
    {
        public IEnumerable<EntityMetadata> Entities =>
        [
            new EntityMetadata(new EntityId(Guid.NewGuid()), "A.Thing", [new PropertyMetadata(1, "X", typeof(int), false)]),
            new EntityMetadata(new EntityId(Guid.NewGuid()), "B.Thing", [new PropertyMetadata(1, "X", typeof(int), false)]),
        ];
    }
}
