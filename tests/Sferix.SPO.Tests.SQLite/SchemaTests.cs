using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.Schema;
using Sferix.SPO.Provider.SQLite;
using Sferix.SPO.Provider.SQLite.Schema;
using Xunit;

namespace Sferix.SPO.Tests.SQLite;

public sealed class SchemaTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"spo-schema-{Guid.NewGuid():N}.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm" })
            File.Delete(file);
    }

    private SqliteConnection Open() => new SqliteProvider().Open($"Data Source={_path}");

    private static EntityMetadata Entity(string clrName, params PropertyMetadata[] properties) =>
        new(new EntityId(Guid.NewGuid()), clrName, properties);

    private static SchemaModel Schema(params EntityMetadata[] entities) =>
        SchemaBuilder.Build(CompiledModel.Build(entities, new SqliteProvider(), new HashSet<Sferix.SPO.Abstractions.Providers.SpoCapability>()));

    private static EntityMetadata Customer() => Entity("App.Customer",
        new PropertyMetadata(2, "Age", typeof(int), true),
        new PropertyMetadata(1, "Name", typeof(string), false));

    private static void Run(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [Fact]
    public void EmptyDatabaseGetsAllTables()
    {
        using var connection = Open();
        SqliteSchemaInitializer.EnsureSchema(connection, Schema(Customer(), Entity("App.Order", new PropertyMetadata(1, "Total", typeof(decimal), false))));
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT group_concat(name) FROM (SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name)";
        Assert.Equal("Customer,Order", command.ExecuteScalar());
    }

    [Fact]
    public void ReopenedMatchingDatabaseIsUnchanged()
    {
        var schema = Schema(Customer());
        using (var connection = Open())
            SqliteSchemaInitializer.EnsureSchema(connection, schema);
        using var reopened = Open();
        SqliteSchemaInitializer.EnsureSchema(reopened, schema);
    }

    [Fact]
    public void SchemaOrderFollowsFieldNumber()
    {
        var table = Assert.Single(Schema(Customer()).Tables);
        Assert.Equal(["Name", "Age"], table.Columns.Select(c => c.Name));
    }

    [Fact]
    public void MissingTableIsReportedAndNothingChanges()
    {
        using var connection = Open();
        SqliteSchemaInitializer.EnsureSchema(connection, Schema(Customer()));
        var ex = Assert.Throws<SpoSchemaMismatchException>(() =>
            SqliteSchemaInitializer.EnsureSchema(connection, Schema(Customer(), Entity("App.Order", new PropertyMetadata(1, "Total", typeof(int), false)))));
        Assert.Equal((SchemaMismatchKind.MissingTable, "Order", "SPO1004"), (ex.Kind, ex.Table, ex.Code));
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table'";
        Assert.Equal(1L, command.ExecuteScalar());
    }

    [Theory]
    [InlineData("CREATE TABLE \"Customer\" (\"Name\" TEXT NOT NULL)", "MissingColumn", "Age")]
    [InlineData("CREATE TABLE \"Customer\" (\"Name\" TEXT NOT NULL, \"Age\" INTEGER, \"Extra\" TEXT)", "ExtraColumn", "Extra")]
    [InlineData("CREATE TABLE \"Customer\" (\"Name\" INTEGER NOT NULL, \"Age\" INTEGER)", "ColumnDiffers", "Name")]
    [InlineData("CREATE TABLE \"Customer\" (\"Name\" TEXT, \"Age\" INTEGER)", "ColumnDiffers", "Name")]
    public void ColumnMismatchIsReportedAndNothingChanges(string existing, string kind, string column)
    {
        using var connection = Open();
        Run(connection, existing);
        var ex = Assert.Throws<SpoSchemaMismatchException>(() => SqliteSchemaInitializer.EnsureSchema(connection, Schema(Customer())));
        Assert.Equal((kind, "Customer", column), (ex.Kind.ToString(), ex.Table, ex.Column));
        Assert.Contains(column, ex.Message);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sql FROM sqlite_master WHERE name = 'Customer'";
        Assert.Equal(existing, command.ExecuteScalar());
    }

    [Fact]
    public void SameTableNameFromTwoNamespacesFailsBeforeAnythingIsWritten()
    {
        var ex = Assert.Throws<SpoModelException>(() => Schema(
            Entity("A.Customer", new PropertyMetadata(1, "Name", typeof(string), false)),
            Entity("B.Customer", new PropertyMetadata(1, "Name", typeof(string), false))));
        Assert.Equal("SPO1005", ex.Code);
        Assert.False(File.Exists(_path));
    }

    [Fact]
    public void UnsupportedClrTypeFailsSchemaBuild()
    {
        var ex = Assert.Throws<SpoModelException>(() => Schema(Entity("App.C", new PropertyMetadata(1, "X", typeof(object), false))));
        Assert.Equal("SPO1006", ex.Code);
    }

    [Theory]
    [InlineData(typeof(bool), "INTEGER")]
    [InlineData(typeof(int), "INTEGER")]
    [InlineData(typeof(long), "INTEGER")]
    [InlineData(typeof(double), "REAL")]
    [InlineData(typeof(decimal), "TEXT")]
    [InlineData(typeof(string), "TEXT")]
    [InlineData(typeof(DateTimeOffset), "TEXT")]
    [InlineData(typeof(Guid), "BLOB")]
    [InlineData(typeof(byte[]), "BLOB")]
    [InlineData(typeof(DateTime), "INTEGER")]
    public void DdlPerStorageType(Type type, string sqlType)
    {
        var table = Assert.Single(Schema(Entity("App.T", new PropertyMetadata(1, "V", type, false))).Tables);
        Assert.Equal($"CREATE TABLE \"T\" (\"V\" {sqlType} NOT NULL)", new SqliteSchemaDialect().RenderCreateTable(table));
    }

    [Theory]
    [InlineData(typeof(byte), SpoStorageType.Int32)]
    [InlineData(typeof(int), SpoStorageType.Int32)]
    [InlineData(typeof(uint), SpoStorageType.Int64)]
    [InlineData(typeof(long), SpoStorageType.Int64)]
    public void EnumMapsToIntegerStorageByUnderlyingType(Type underlying, SpoStorageType expected)
    {
        var column = Assert.Single(Assert.Single(Schema(Entity("App.T", new PropertyMetadata(1, "V", typeof(DayOfWeek), true, underlying))).Tables).Columns);
        Assert.Equal(expected, column.StorageType);
    }

    [Fact]
    public void DdlIncludesNullabilityAndPrimaryKey()
    {
        var table = new SchemaTable("T", [new("Id", SpoStorageType.Guid, false), new("Note", SpoStorageType.String, true)], ["Id"]);
        Assert.Equal("CREATE TABLE \"T\" (\"Id\" BLOB NOT NULL, \"Note\" TEXT, PRIMARY KEY (\"Id\"))", new SqliteSchemaDialect().RenderCreateTable(table));
    }

    [Fact]
    public void DialectRejectsUnsafeIdentifier()
    {
        var table = new SchemaTable("T\"; DROP TABLE X; --", [new("V", SpoStorageType.Int32, false)]);
        Assert.Throws<ArgumentException>(() => new SqliteSchemaDialect().RenderCreateTable(table));
    }

    [Fact]
    public void MismatchMessageIsLocalized()
    {
        var previous = System.Globalization.CultureInfo.CurrentUICulture;
        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("de");
            Assert.Contains("fehlt", new SpoSchemaMismatchException(SchemaMismatchKind.MissingTable, "T").Message);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = previous;
        }
    }
}
