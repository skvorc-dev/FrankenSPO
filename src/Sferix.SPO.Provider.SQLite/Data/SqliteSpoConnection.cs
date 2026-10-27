using System.Text;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Abstractions.Query;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Provider.SQLite.Resources;
using Sferix.SPO.Provider.SQLite.Schema;

namespace Sferix.SPO.Provider.SQLite.Data;

/// <summary>Row access on one SQLite connection.</summary>
/// <remarks>
/// SQL text is built only from identifiers that <see cref="SqliteSchemaDialect.Quote"/> validates; every value and the key are parameters.
/// The driver has no truly asynchronous I/O, so each async member checks cancellation and then runs its sync twin.
/// </remarks>
internal sealed class SqliteSpoConnection(SqliteConnection connection) : ISpoConnection
{
    private SqliteSpoTransaction? _current;

    internal void TransactionEnded(SqliteSpoTransaction transaction)
    {
        if (ReferenceEquals(_current, transaction))
            _current = null;
    }

    public void EnsureSchema(SchemaModel model) => SqliteSchemaInitializer.EnsureSchema(connection, model);

    public Task EnsureSchemaAsync(SchemaModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureSchema(model);
        return Task.CompletedTask;
    }

    public ISpoTransaction BeginTransaction()
    {
        var transaction = new SqliteSpoTransaction(connection.BeginTransaction(), this);
        _current = transaction;
        return transaction;
    }

    public Task<ISpoTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(BeginTransaction());
    }

    public IReadOnlyList<object?>? ReadRow(SchemaTable table, Guid key)
    {
        ArgumentNullException.ThrowIfNull(table);
        var keyColumn = KeyColumn(table);
        using var command = connection.CreateCommand();
        command.Transaction = _current?.Inner;
        command.CommandText = $"SELECT {string.Join(", ", table.Columns.Select(c => SqliteSchemaDialect.Quote(c.Name)))} " +
                              $"FROM {SqliteSchemaDialect.Quote(table.Name)} WHERE {SqliteSchemaDialect.Quote(keyColumn)} = $key";
        command.Parameters.AddWithValue("$key", SqliteValueConverter.KeyToStored(key));
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;
        var row = new object?[table.Columns.Count];
        for (var i = 0; i < row.Length; i++)
            row[i] = SqliteValueConverter.FromStored(table.Columns[i].StorageType, reader, i);
        return row;
    }

    public Task<IReadOnlyList<object?>?> ReadRowAsync(SchemaTable table, Guid key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadRow(table, key));
    }

    public IReadOnlyList<IReadOnlyList<object?>> Query(SpoQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        using var command = connection.CreateCommand();
        command.Transaction = _current?.Inner;
        SqliteQueryRenderer.Render(command, query);
        using var reader = command.ExecuteReader();
        var rows = new List<IReadOnlyList<object?>>();
        while (reader.Read())
        {
            var row = new object?[query.Table.Columns.Count];
            for (var i = 0; i < row.Length; i++)
                row[i] = SqliteValueConverter.FromStored(query.Table.Columns[i].StorageType, reader, i);
            rows.Add(row);
        }
        return rows;
    }

    public Task<IReadOnlyList<IReadOnlyList<object?>>> QueryAsync(SpoQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Query(query));
    }

    public void Insert(ISpoTransaction transaction, SchemaTable table, IReadOnlyList<object?> values)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != table.Columns.Count)
            throw new ArgumentException(SqliteMessages.ValueCountMismatch(table.Name, table.Columns.Count, values.Count), nameof(values));

        // Convert everything first so a rejected value writes nothing.
        var stored = new object[values.Count];
        for (var i = 0; i < stored.Length; i++)
            stored[i] = SqliteValueConverter.ToStored(table.Columns[i].StorageType, values[i], table.Name, table.Columns[i].Name);

        using var command = connection.CreateCommand();
        command.Transaction = ((SqliteSpoTransaction)transaction).Inner;
        command.CommandText = $"INSERT INTO {SqliteSchemaDialect.Quote(table.Name)} " +
                              $"({string.Join(", ", table.Columns.Select(c => SqliteSchemaDialect.Quote(c.Name)))}) " +
                              $"VALUES ({string.Join(", ", stored.Select((_, i) => "$p" + i))})";
        for (var i = 0; i < stored.Length; i++)
            command.Parameters.AddWithValue("$p" + i, stored[i]);
        command.ExecuteNonQuery();
    }

    public Task InsertAsync(ISpoTransaction transaction, SchemaTable table, IReadOnlyList<object?> values, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Insert(transaction, table, values);
        return Task.CompletedTask;
    }

    public int Update(ISpoTransaction transaction, SchemaTable table, Guid key, IReadOnlyList<SpoColumnValue> changes)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(changes);
        var keyColumn = KeyColumn(table);
        if (changes.Count == 0)
            return 0;

        var assignments = new StringBuilder();
        var stored = new object[changes.Count];
        for (var i = 0; i < changes.Count; i++)
        {
            var column = table.Columns.FirstOrDefault(c => string.Equals(c.Name, changes[i].Column, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException(SqliteMessages.UnknownColumn(table.Name, changes[i].Column), nameof(changes));
            stored[i] = SqliteValueConverter.ToStored(column.StorageType, changes[i].Value, table.Name, column.Name);
            if (i > 0) assignments.Append(", ");
            assignments.Append(SqliteSchemaDialect.Quote(column.Name)).Append(" = $p").Append(i);
        }

        using var command = connection.CreateCommand();
        command.Transaction = ((SqliteSpoTransaction)transaction).Inner;
        command.CommandText = $"UPDATE {SqliteSchemaDialect.Quote(table.Name)} SET {assignments} WHERE {SqliteSchemaDialect.Quote(keyColumn)} = $key";
        for (var i = 0; i < stored.Length; i++)
            command.Parameters.AddWithValue("$p" + i, stored[i]);
        command.Parameters.AddWithValue("$key", SqliteValueConverter.KeyToStored(key));
        return command.ExecuteNonQuery();
    }

    public Task<int> UpdateAsync(ISpoTransaction transaction, SchemaTable table, Guid key, IReadOnlyList<SpoColumnValue> changes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Update(transaction, table, key, changes));
    }

    public void Dispose()
    {
        _current?.Dispose();
        connection.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private static string KeyColumn(SchemaTable table) =>
        table.PrimaryKey.Count == 1
            ? table.PrimaryKey[0]
            : throw new InvalidOperationException(SqliteMessages.KeyColumnRequired(table.Name));
}
