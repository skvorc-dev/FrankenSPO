using Xunit;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Provider.SQLite;

namespace Sferix.SPO.Tests.SQLite;

public sealed class ConnectionTests
{
    [Fact]
    public void OpensRealInMemorySqliteDatabase()
    {
        using var connection = new SqliteProvider().Open("Data Source=:memory:");
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sqlite_version()";
        Assert.False(string.IsNullOrWhiteSpace(command.ExecuteScalar()?.ToString()));
    }

    [Fact]
    public void UserInputRemainsDataWhenParameterized()
    {
        using var connection = new SqliteProvider().Open("Data Source=:memory:");
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT $input";
        var attack = "'; DROP TABLE anything; --";
        command.Parameters.AddWithValue("$input", attack);
        Assert.Equal(attack, command.ExecuteScalar());
    }

    [Fact]
    public void ProviderDeclaresAtomicSaveChanges()
    {
        var provider = new SqliteProvider();
        Assert.Equal("SQLite", provider.Name);
        Assert.Equal([SpoCapability.AtomicSaveChanges], provider.Capabilities);
    }

    [Fact]
    public void FileDatabaseUsesWalForeignKeysAndBusyTimeout()
    {
        var path = Path.Combine(Path.GetTempPath(), $"spo-{Guid.NewGuid():N}.db");
        try
        {
            using (var connection = new SqliteProvider().Open($"Data Source={path}"))
            {
                Assert.Equal("wal", Scalar(connection, "PRAGMA journal_mode"));
                Assert.Equal(1L, Scalar(connection, "PRAGMA foreign_keys"));
                Assert.Equal(5000L, Scalar(connection, "PRAGMA busy_timeout"));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            foreach (var file in new[] { path, path + "-wal", path + "-shm" })
                File.Delete(file);
        }
    }

    [Fact]
    public void MemoryDatabaseKeepsForeignKeysOnWithoutWal()
    {
        using var connection = new SqliteProvider().Open("Data Source=:memory:");
        Assert.Equal(1L, Scalar(connection, "PRAGMA foreign_keys"));
        Assert.Equal("memory", Scalar(connection, "PRAGMA journal_mode"));
    }

    private static object? Scalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }
}
