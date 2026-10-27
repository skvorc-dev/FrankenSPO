namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A provider transaction. Disposing it without a commit rolls it back.</summary>
public interface ISpoTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>Makes all changes of the transaction permanent.</summary>
    void Commit();

    /// <summary>Makes all changes of the transaction permanent.</summary>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>Discards all changes of the transaction.</summary>
    void Rollback();

    /// <summary>Discards all changes of the transaction.</summary>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task RollbackAsync(CancellationToken cancellationToken);
}
