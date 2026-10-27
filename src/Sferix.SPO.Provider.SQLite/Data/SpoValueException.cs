using Sferix.SPO.Provider.SQLite.Resources;

namespace Sferix.SPO.Provider.SQLite.Data;

/// <summary>Thrown when a value cannot be stored exactly; nothing has been written.</summary>
/// <remarks>Internal until a public API throws it (D-42). The message names table and column only, never the value.</remarks>
internal sealed class SpoValueException : Exception
{
    public const string DateTimeNotUtcCode = "SPO1007";

    private SpoValueException(string code, string message, string table, string column) : base(message)
    {
        Code = code;
        Table = table;
        Column = column;
    }

    public string Code { get; }
    public string Table { get; }
    public string Column { get; }

    public static SpoValueException DateTimeNotUtc(string table, string column) =>
        new(DateTimeNotUtcCode, SqliteMessages.DateTimeNotUtc(table, column), table, column);
}
