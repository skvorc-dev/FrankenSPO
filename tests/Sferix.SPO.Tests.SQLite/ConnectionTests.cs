using Xunit;
using Sferix.SPO.Provider.SQLite;

namespace Sferix.SPO.Tests.SQLite;

public sealed class ConnectionTests
{
    [Fact]
    public void OpensRealInMemorySqliteDatabase()
    {
        using var connection = SqliteConnectionFactory.Open("Data Source=:memory:");
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sqlite_version()";
        Assert.False(string.IsNullOrWhiteSpace(command.ExecuteScalar()?.ToString()));
    }

    [Fact]
    public void UserInputRemainsDataWhenParameterized()
    {
        using var connection = SqliteConnectionFactory.Open("Data Source=:memory:");
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT $input";
        var attack = "'; DROP TABLE anything; --";
        command.Parameters.AddWithValue("$input", attack);
        Assert.Equal(attack, command.ExecuteScalar());
    }
}
