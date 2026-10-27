using Sferix.SPO.Provider.SQLite.Resources;

namespace Sferix.SPO.Provider.SQLite.Schema;

/// <summary>Thrown when an existing database does not match the model; nothing has been changed.</summary>
/// <remarks>Internal until a public API throws it (D-42). It names the table and column only, never SQL or values.</remarks>
internal sealed class SpoSchemaMismatchException : Exception
{
    public const string ErrorCode = "SPO1004";

    public SpoSchemaMismatchException(SchemaMismatchKind kind, string table, string? column = null)
        : base(SqliteMessages.Mismatch(kind, table, column))
    {
        Kind = kind;
        Table = table;
        Column = column;
    }

    public string Code => ErrorCode;
    public SchemaMismatchKind Kind { get; }
    public string Table { get; }
    public string? Column { get; }
}

internal enum SchemaMismatchKind
{
    MissingTable,
    MissingColumn,
    ExtraColumn,
    ColumnDiffers,
}
