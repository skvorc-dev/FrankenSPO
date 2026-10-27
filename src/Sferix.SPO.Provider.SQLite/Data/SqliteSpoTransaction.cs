using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Providers;

namespace Sferix.SPO.Provider.SQLite.Data;

/// <summary>A SQLite transaction; disposing it without a commit rolls it back.</summary>
/// <remarks>SQLite has no truly asynchronous I/O in the driver, so the async members check cancellation and then run the same code.</remarks>
internal sealed class SqliteSpoTransaction(SqliteTransaction inner, SqliteSpoConnection owner) : ISpoTransaction
{
    public SqliteTransaction Inner { get; } = inner;

    public void Commit()
    {
        Inner.Commit();
        owner.TransactionEnded(this);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Commit();
        return Task.CompletedTask;
    }

    public void Rollback()
    {
        Inner.Rollback();
        owner.TransactionEnded(this);
    }

    public Task RollbackAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Rollback();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Inner.Dispose();
        owner.TransactionEnded(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
