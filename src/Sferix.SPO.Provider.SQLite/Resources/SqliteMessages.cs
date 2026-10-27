using System.Globalization;
using System.Resources;
using Sferix.SPO.Provider.SQLite.Schema;

namespace Sferix.SPO.Provider.SQLite.Resources;

/// <summary>Reads the SQLite provider's user-visible messages from the embedded <c>SqliteMessages</c> resources (D-40, D-41).</summary>
/// <remarks>Messages never contain SQL text or values. An unsupported culture falls back to English.</remarks>
internal static class SqliteMessages
{
    private static readonly ResourceManager Resources =
        new("Sferix.SPO.Provider.SQLite.Resources.SqliteMessages", typeof(SqliteMessages).Assembly);

    internal static string Mismatch(SchemaMismatchKind kind, string table, string? column) =>
        string.Format(CultureInfo.CurrentUICulture, Get("Schema" + kind), table, column);

    private static string Get(string name) =>
        Resources.GetString(name, CultureInfo.CurrentUICulture)
        ?? throw new InvalidOperationException($"Missing SQLite provider resource '{name}'.");
}
