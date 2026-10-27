using Sferix.SPO.Abstractions.Query;
using Sferix.SPO.Abstractions.Schema;

namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A provider connection with row access by key (P1: one <see cref="Guid"/> key column).</summary>
/// <remarks>
/// Values are CLR values of the column's <see cref="SpoStorageType"/> (<c>int</c> for Int32, <c>long</c> for Int64, and so on) in schema column
/// order; enums arrive already converted to their integer. Every value reaches the database as a parameter. A connection is not thread-safe.
/// This is not a query API and takes no SQL.
/// </remarks>
public interface ISpoConnection : IDisposable, IAsyncDisposable
{
    /// <summary>Creates the schema in an empty database, or checks that an existing one matches.</summary>
    /// <param name="model">The schema the model needs.</param>
    void EnsureSchema(SchemaModel model);

    /// <summary>Creates the schema in an empty database, or checks that an existing one matches.</summary>
    /// <param name="model">The schema the model needs.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task EnsureSchemaAsync(SchemaModel model, CancellationToken cancellationToken);

    /// <summary>Starts a transaction; later reads on this connection take part in it.</summary>
    ISpoTransaction BeginTransaction();

    /// <summary>Starts a transaction; later reads on this connection take part in it.</summary>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task<ISpoTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>Reads one row by its key.</summary>
    /// <param name="table">The table; it needs exactly one primary-key column.</param>
    /// <param name="key">The key value.</param>
    /// <returns>The values in schema column order, or null when there is no such row.</returns>
    IReadOnlyList<object?>? ReadRow(SchemaTable table, Guid key);

    /// <summary>Reads one row by its key.</summary>
    /// <param name="table">The table; it needs exactly one primary-key column.</param>
    /// <param name="key">The key value.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task<IReadOnlyList<object?>?> ReadRowAsync(SchemaTable table, Guid key, CancellationToken cancellationToken);

    /// <summary>Inserts one row.</summary>
    /// <param name="transaction">The transaction to run in.</param>
    /// <param name="table">The table.</param>
    /// <param name="values">One value per column, in schema column order.</param>
    void Insert(ISpoTransaction transaction, SchemaTable table, IReadOnlyList<object?> values);

    /// <summary>Inserts one row.</summary>
    /// <param name="transaction">The transaction to run in.</param>
    /// <param name="table">The table.</param>
    /// <param name="values">One value per column, in schema column order.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task InsertAsync(ISpoTransaction transaction, SchemaTable table, IReadOnlyList<object?> values, CancellationToken cancellationToken);

    /// <summary>Changes the given columns of the row with the given key.</summary>
    /// <param name="transaction">The transaction to run in.</param>
    /// <param name="table">The table; it needs exactly one primary-key column.</param>
    /// <param name="key">The key value.</param>
    /// <param name="changes">The columns to change; other columns stay as they are.</param>
    /// <returns>The number of rows affected: 1, or 0 when there is no such row.</returns>
    int Update(ISpoTransaction transaction, SchemaTable table, Guid key, IReadOnlyList<SpoColumnValue> changes);

    /// <summary>Changes the given columns of the row with the given key.</summary>
    /// <param name="transaction">The transaction to run in.</param>
    /// <param name="table">The table; it needs exactly one primary-key column.</param>
    /// <param name="key">The key value.</param>
    /// <param name="changes">The columns to change; other columns stay as they are.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    /// <returns>The number of rows affected: 1, or 0 when there is no such row.</returns>
    Task<int> UpdateAsync(ISpoTransaction transaction, SchemaTable table, Guid key, IReadOnlyList<SpoColumnValue> changes, CancellationToken cancellationToken);

    /// <summary>Runs a query and returns whole rows, each in schema column order.</summary>
    /// <param name="query">The neutral query; values are parameters, column names come from its table.</param>
    /// <remarks>Reads take part in the connection's current transaction.</remarks>
    IReadOnlyList<IReadOnlyList<object?>> Query(SpoQuery query);

    /// <summary>Runs a query and returns whole rows, each in schema column order.</summary>
    /// <param name="query">The neutral query.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task<IReadOnlyList<IReadOnlyList<object?>>> QueryAsync(SpoQuery query, CancellationToken cancellationToken);
}
