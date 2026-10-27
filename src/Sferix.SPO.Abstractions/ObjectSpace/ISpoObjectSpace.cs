using Sferix.SPO.Abstractions.Entities;

namespace Sferix.SPO.Abstractions.ObjectSpace;

/// <summary>A unit of work over one connection: tracks entities, loads them by key and saves changes atomically (D-29).</summary>
/// <remarks>
/// Not thread-safe: a call made while another call is running (re-entrant or from another thread) throws a
/// <c>SpoObjectSpaceException</c> with code SPO1010, and any call after disposal throws <see cref="ObjectDisposedException"/>.
/// Each <c>SaveChanges</c> runs in its own transaction. Sync and async members share one code path.
/// There is no delete, no query and no version check yet.
/// </remarks>
public interface ISpoObjectSpace : IDisposable, IAsyncDisposable
{
    /// <summary>Starts tracking a new entity; it is inserted by the next save.</summary>
    /// <typeparam name="T">The entity type; it must be part of the model.</typeparam>
    /// <param name="entity">The entity. An empty <see cref="SPOBaseObject.Id"/> gets a new UUIDv7 from the model's time provider.</param>
    /// <returns><paramref name="entity"/>.</returns>
    T Add<T>(T entity) where T : SPOBaseObject;

    /// <summary>Returns the entity with the given key: the tracked instance if there is one, otherwise it is loaded.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="id">The key.</param>
    /// <returns>The entity, or null when there is no such row.</returns>
    T? Get<T>(Guid id) where T : SPOBaseObject;

    /// <summary>Returns the entity with the given key: the tracked instance if there is one, otherwise it is loaded.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="id">The key.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    /// <returns>The entity, or null when there is no such row.</returns>
    Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken) where T : SPOBaseObject;

    /// <summary>Writes all additions and changes in one transaction.</summary>
    /// <remarks>On failure nothing is written, values set by the save are restored, and the changes stay pending.</remarks>
    void SaveChanges();

    /// <summary>Writes all additions and changes in one transaction.</summary>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
