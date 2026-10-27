using System.Text;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Core.Sql;

namespace Sferix.SPO.Provider.SQLite.Schema;

/// <summary>Renders SQLite DDL from the neutral schema model.</summary>
/// <remarks>Identifiers are validated with <see cref="SqlIdentifier"/> and quoted. It renders text only.</remarks>
internal sealed class SqliteSchemaDialect : ISpoSchemaDialect
{
    public string RenderCreateTable(SchemaTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        var sql = new StringBuilder("CREATE TABLE ").Append(Quote(table.Name)).Append(" (");
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            if (i > 0) sql.Append(", ");
            sql.Append(Quote(column.Name)).Append(' ').Append(TypeName(column.StorageType));
            if (!column.IsNullable) sql.Append(" NOT NULL");
        }
        if (table.PrimaryKey.Count > 0)
            sql.Append(", PRIMARY KEY (").Append(string.Join(", ", table.PrimaryKey.Select(Quote))).Append(')');
        return sql.Append(')').ToString();
    }

    public static string TypeName(SpoStorageType type) => type switch
    {
        SpoStorageType.Boolean or SpoStorageType.Int32 or SpoStorageType.Int64 or SpoStorageType.DateTime => "INTEGER",
        SpoStorageType.Double => "REAL",
        SpoStorageType.Decimal or SpoStorageType.String or SpoStorageType.DateTimeOffset => "TEXT",
        SpoStorageType.Guid or SpoStorageType.Binary => "BLOB",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    private static string Quote(string identifier) => "\"" + new SqlIdentifier(identifier).Value + "\"";
}
