using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Schema;

namespace Sferix.SPO.Provider.SQLite.Schema;

/// <summary>Creates the schema in an empty database or checks that an existing one matches the model.</summary>
/// <remarks>No migrations: any difference throws <see cref="SpoSchemaMismatchException"/> and nothing is changed. Tables that are not in the model are ignored.</remarks>
internal static class SqliteSchemaInitializer
{
    public static void EnsureSchema(SqliteConnection connection, SchemaModel model)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(model);
        var dialect = new SqliteSchemaDialect();

        using var transaction = connection.BeginTransaction();
        var existing = TableNames(connection, transaction);
        if (existing.Count == 0)
        {
            foreach (var table in model.Tables)
                Execute(connection, transaction, dialect.RenderCreateTable(table));
            transaction.Commit();
            return;
        }

        foreach (var table in model.Tables)
        {
            if (!existing.Contains(table.Name))
                throw new SpoSchemaMismatchException(SchemaMismatchKind.MissingTable, table.Name);
            Check(connection, transaction, table);
        }
        transaction.Commit();
    }

    private static void Check(SqliteConnection connection, SqliteTransaction transaction, SchemaTable table)
    {
        var actual = new Dictionary<string, (string Type, bool NotNull)>(StringComparer.OrdinalIgnoreCase);
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            // Table name comes from validated metadata and is passed to the pragma function as a parameter.
            command.CommandText = "SELECT name, type, \"notnull\" FROM pragma_table_info($table)";
            command.Parameters.AddWithValue("$table", table.Name);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                actual[reader.GetString(0)] = (reader.GetString(1), reader.GetInt64(2) != 0);
        }

        foreach (var column in table.Columns)
        {
            if (!actual.Remove(column.Name, out var found))
                throw new SpoSchemaMismatchException(SchemaMismatchKind.MissingColumn, table.Name, column.Name);
            if (!string.Equals(found.Type, SqliteSchemaDialect.TypeName(column.StorageType), StringComparison.OrdinalIgnoreCase)
                || found.NotNull != !column.IsNullable)
                throw new SpoSchemaMismatchException(SchemaMismatchKind.ColumnDiffers, table.Name, column.Name);
        }

        if (actual.Count > 0)
            throw new SpoSchemaMismatchException(SchemaMismatchKind.ExtraColumn, table.Name, actual.Keys.Order().First());
    }

    private static HashSet<string> TableNames(SqliteConnection connection, SqliteTransaction transaction)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            names.Add(reader.GetString(0));
        return names;
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
