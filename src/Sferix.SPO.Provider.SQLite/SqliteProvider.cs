using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Providers;

namespace Sferix.SPO.Provider.SQLite;

/// <summary>The SQLite provider: declares its capabilities and opens configured connections.</summary>
/// <remarks>
/// Every connection gets <c>foreign_keys=ON</c> and a busy timeout; file databases also get the WAL journal (D-34).
/// This does not implement persistence or ObjectSpace.
/// </remarks>
public sealed class SqliteProvider : ISpoProvider
{
    private const int BusyTimeoutMilliseconds = 5000;

    private static readonly IReadOnlySet<SpoCapability> Supported =
        new HashSet<SpoCapability> { SpoCapability.AtomicSaveChanges };

    /// <inheritdoc />
    public string Name => "SQLite";

    /// <inheritdoc />
    public IReadOnlySet<SpoCapability> Capabilities => Supported;

    /// <summary>Opens a connection and applies the provider's settings.</summary>
    /// <param name="connectionString">A SQLite connection string; use <c>:memory:</c> or a temp file in tests.</param>
    public SqliteConnection Open(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var connection = new SqliteConnection(connectionString);
        try
        {
            connection.Open();
            Execute(connection, "PRAGMA foreign_keys = ON");
            Execute(connection, $"PRAGMA busy_timeout = {BusyTimeoutMilliseconds}");
            if (IsFileDatabase(builder))
                Execute(connection, "PRAGMA journal_mode = WAL");
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static bool IsFileDatabase(SqliteConnectionStringBuilder builder) =>
        builder.Mode != SqliteOpenMode.Memory
        && !string.IsNullOrEmpty(builder.DataSource)
        && builder.DataSource != ":memory:"
        && !builder.DataSource.Contains("mode=memory", StringComparison.OrdinalIgnoreCase);

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
