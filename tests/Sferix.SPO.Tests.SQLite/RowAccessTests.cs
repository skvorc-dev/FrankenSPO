using System.Globalization;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Provider.SQLite;
using Sferix.SPO.Provider.SQLite.Data;
using Xunit;

namespace Sferix.SPO.Tests.SQLite;

public sealed class RowAccessTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"spo-rows-{Guid.NewGuid():N}.db");

    private static readonly SchemaTable All = new("AllTypes",
    [
        new("Id", SpoStorageType.Guid, false),
        new("Flag", SpoStorageType.Boolean, true),
        new("Small", SpoStorageType.Int32, true),
        new("Big", SpoStorageType.Int64, true),
        new("Price", SpoStorageType.Decimal, true),
        new("Ratio", SpoStorageType.Double, true),
        new("Title", SpoStorageType.String, true),
        new("Key", SpoStorageType.Guid, true),
        new("Moment", SpoStorageType.DateTime, true),
        new("Offset", SpoStorageType.DateTimeOffset, true),
        new("Blob", SpoStorageType.Binary, true),
    ], ["Id"]);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm" })
            File.Delete(file);
    }

    private ISpoConnection Open(params SchemaTable[] tables)
    {
        var connection = ((ISpoProvider)new SqliteProvider()).Open($"Data Source={_path}");
        connection.EnsureSchema(new SchemaModel(tables.Length == 0 ? [All] : tables));
        return connection;
    }

    private static object?[] Row(Guid id) =>
    [
        id, true, 7, 1L << 40, 1.50m, 0.25, "text", Guid.NewGuid(),
        new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc).AddTicks(1234),
        new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.FromHours(-3)).AddTicks(99), new byte[] { 0, 1, 255 },
    ];

    private static void Save(ISpoConnection connection, SchemaTable table, IReadOnlyList<object?> values)
    {
        using var tx = connection.BeginTransaction();
        connection.Insert(tx, table, values);
        tx.Commit();
    }

    [Fact]
    public void EveryStorageTypeRoundTrips()
    {
        using var connection = Open();
        var row = Row(Guid.CreateVersion7());
        Save(connection, All, row);

        var read = connection.ReadRow(All, (Guid)row[0]!)!;
        for (var i = 0; i < row.Length; i++)
        {
            if (row[i] is byte[] bytes)
                Assert.Equal(bytes, (byte[])read[i]!);
            else
                Assert.Equal(row[i], read[i]);
        }
        Assert.Equal(DateTimeKind.Utc, ((DateTime)read[8]!).Kind);
        Assert.Equal(TimeSpan.FromHours(-3), ((DateTimeOffset)read[9]!).Offset);
        Assert.Equal("1.50", ((decimal)read[4]!).ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void NullsRoundTripAndMissingRowIsNull()
    {
        using var connection = Open();
        var id = Guid.CreateVersion7();
        Save(connection, All, [id, null, null, null, null, null, null, null, null, null, null]);
        var read = connection.ReadRow(All, id)!;
        Assert.Equal(id, read[0]);
        Assert.All(read.Skip(1), Assert.Null);
        Assert.Null(connection.ReadRow(All, Guid.NewGuid()));
    }

    [Fact]
    public void EnumValuesAreStoredAsTheirIntegers()
    {
        using var connection = Open();
        var id = Guid.CreateVersion7();
        Save(connection, All, [id, null, (int)DayOfWeek.Friday, (long)DayOfWeek.Friday, null, null, null, null, null, null, null]);
        var read = connection.ReadRow(All, id)!;
        Assert.Equal(DayOfWeek.Friday, (DayOfWeek)(int)read[2]!);
        Assert.Equal(5L, read[3]);
    }

    [Fact]
    public void LaterUuidV7SortsLaterInTheDatabase()
    {
        using var connection = Open();
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var ids = Enumerable.Range(0, 20).Select(i => Guid.CreateVersion7(start.AddMinutes(i * 1000 + 1))).ToArray();
        foreach (var id in ids.Reverse())
            Save(connection, All, [id, null, null, null, null, null, null, null, null, null, null]);

        using var raw = new SqliteConnection($"Data Source={_path}");
        raw.Open();
        using var command = raw.CreateCommand();
        command.CommandText = "SELECT Id FROM AllTypes ORDER BY Id";
        using var reader = command.ExecuteReader();
        var sorted = new List<Guid>();
        while (reader.Read())
            sorted.Add(new Guid((byte[])reader.GetValue(0), bigEndian: true));
        Assert.Equal(ids, sorted);
    }

    [Fact]
    public void NonUtcDateTimeFailsWithCodeAndWritesNothing()
    {
        using var connection = Open();
        var row = Row(Guid.CreateVersion7());
        row[8] = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);
        using var tx = connection.BeginTransaction();
        var ex = Assert.Throws<SpoValueException>(() => connection.Insert(tx, All, row));
        Assert.Equal(("SPO1007", "AllTypes", "Moment"), (ex.Code, ex.Table, ex.Column));
        tx.Commit();
        Assert.Null(connection.ReadRow(All, (Guid)row[0]!));

        row[8] = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        using var tx2 = connection.BeginTransaction();
        Assert.Throws<SpoValueException>(() => connection.Insert(tx2, All, row));
    }

    [Fact]
    public void NonUtcDateTimeMessageIsLocalized()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("de");
            Assert.Contains("nicht UTC", SpoValueException.DateTimeNotUtc("T", "C").Message);
            CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");
            Assert.Contains("not UTC", SpoValueException.DateTimeNotUtc("T", "C").Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void UpdateChangesOnlyTheGivenColumns()
    {
        using var connection = Open();
        var row = Row(Guid.CreateVersion7());
        var id = (Guid)row[0]!;
        Save(connection, All, row);

        using (var tx = connection.BeginTransaction())
        {
            Assert.Equal(1, connection.Update(tx, All, id, [new("Title", "changed"), new("Small", null)]));
            Assert.Equal(0, connection.Update(tx, All, Guid.NewGuid(), [new("Title", "x")]));
            tx.Commit();
        }

        var read = connection.ReadRow(All, id)!;
        Assert.Equal("changed", read[6]);
        Assert.Null(read[2]);
        Assert.Equal(row[3], read[3]);
        Assert.Equal(row[4], read[4]);
        Assert.Equal(row[9], read[9]);
    }

    [Fact]
    public void RollbackLeavesTheTableUnchanged()
    {
        using var connection = Open();
        var row = Row(Guid.CreateVersion7());
        var id = (Guid)row[0]!;
        Save(connection, All, row);

        using (var tx = connection.BeginTransaction())
        {
            connection.Update(tx, All, id, [new("Title", "changed")]);
            connection.Insert(tx, All, Row(Guid.CreateVersion7()));
            Assert.Equal("changed", connection.ReadRow(All, id)![6]);
            tx.Rollback();
        }

        Assert.Equal("text", connection.ReadRow(All, id)![6]);
        using var raw = new SqliteConnection($"Data Source={_path}");
        raw.Open();
        using var count = raw.CreateCommand();
        count.CommandText = "SELECT count(*) FROM AllTypes";
        Assert.Equal(1L, count.ExecuteScalar());
    }

    [Fact]
    public void DisposingATransactionWithoutCommitRollsBack()
    {
        using var connection = Open();
        var id = Guid.CreateVersion7();
        using (var tx = connection.BeginTransaction())
            connection.Insert(tx, All, [id, null, null, null, null, null, null, null, null, null, null]);
        Assert.Null(connection.ReadRow(All, id));
    }

    [Theory]
    [InlineData("O'Brien")]
    [InlineData("' OR 1=1 --")]
    [InlineData("a; DROP TABLE AllTypes; --")]
    [InlineData("100% _sure_ \\ \"quoted\"")]
    [InlineData("Robert'); DROP TABLE AllTypes;--")]
    public void HostileTextIsStoredAndReadBackLiterally(string text)
    {
        using var connection = Open();
        var id = Guid.CreateVersion7();
        Save(connection, All, [id, null, null, null, null, null, text, null, null, null, null]);
        Assert.Equal(text, connection.ReadRow(All, id)![6]);

        using (var tx = connection.BeginTransaction())
        {
            Assert.Equal(1, connection.Update(tx, All, id, [new("Title", text + "!")]));
            tx.Commit();
        }
        Assert.Equal(text + "!", connection.ReadRow(All, id)![6]);
        Assert.NotNull(connection.ReadRow(All, id));
    }

    [Fact]
    public void UnsafeColumnNameIsRejectedBeforeAnySql()
    {
        using var connection = Open();
        using var tx = connection.BeginTransaction();
        Assert.Throws<ArgumentException>(() => connection.Update(tx, All, Guid.NewGuid(), [new("Title\" = 1; --", "x")]));
    }

    [Fact]
    public void TableWithoutSingleKeyColumnIsRejected()
    {
        var keyless = new SchemaTable("Keyless", [new("A", SpoStorageType.Int32, false)]);
        using var connection = Open(keyless);
        Assert.Throws<InvalidOperationException>(() => connection.ReadRow(keyless, Guid.NewGuid()));
    }

    [Fact]
    public void WrongValueCountIsRejected()
    {
        using var connection = Open();
        using var tx = connection.BeginTransaction();
        Assert.Throws<ArgumentException>(() => connection.Insert(tx, All, [Guid.NewGuid()]));
    }

    [Fact]
    public async Task AsyncTwinsWorkAndHonourCancellation()
    {
        await using var connection = Open();
        var row = Row(Guid.CreateVersion7());
        var id = (Guid)row[0]!;
        await using (var tx = await connection.BeginTransactionAsync(CancellationToken.None))
        {
            await connection.InsertAsync(tx, All, row, CancellationToken.None);
            Assert.Equal(1, await connection.UpdateAsync(tx, All, id, [new("Title", "async")], CancellationToken.None));
            await tx.CommitAsync(CancellationToken.None);
        }
        Assert.Equal("async", (await connection.ReadRowAsync(All, id, CancellationToken.None))![6]);

        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => connection.ReadRowAsync(All, id, cancelled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => connection.BeginTransactionAsync(cancelled.Token));
    }
}
