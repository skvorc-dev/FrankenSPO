using Microsoft.Data.Sqlite;

namespace Sferix.SPO.Provider.SQLite;

/// <summary>Creates a SQLite connection. This does not implement persistence or ObjectSpace.</summary>
public static class SqliteConnectionFactory
{
    public static SqliteConnection Open(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var connection = new SqliteConnection(connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
