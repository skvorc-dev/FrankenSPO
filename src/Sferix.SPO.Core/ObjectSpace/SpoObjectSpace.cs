using Sferix.SPO.Abstractions.Entities;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Abstractions.Providers;
using System.Linq.Expressions;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.Query;
using Sferix.SPO.Core.Resources;

namespace Sferix.SPO.Core.ObjectSpace;

/// <summary>The ObjectSpace: identity map by entity + key, additions, snapshot-based change detection, atomic save.</summary>
/// <remarks>
/// One code path serves sync and async: the sync members run the same method with <c>async: false</c>, which then never awaits,
/// so nothing blocks on a pending task. After a failed save the user's values and pending changes stay and the committed
/// snapshots do not move (D-30); only values the save itself set (CreatedAt, ModifiedAt) are restored.
/// </remarks>
internal sealed class SpoObjectSpace(SpoModel model, ISpoConnection connection) : ISpoObjectSpace
{
    private sealed class Tracked(EntityInfo info, SPOBaseObject entity, object?[]? snapshot)
    {
        public Guid Key { get; } = entity.Id;
        public EntityInfo Info { get; } = info;
        public SPOBaseObject Entity { get; } = entity;
        public bool PendingDelete { get; set; }
        public object?[]? Snapshot { get; set; } = snapshot; // null = added, not yet saved
    }

    private readonly Dictionary<(string Entity, Guid Id), Tracked> map = [];
    private readonly List<Tracked> added = [];
    private int busy;
    private bool disposed;

    public T Add<T>(T entity) where T : SPOBaseObject
    {
        ArgumentNullException.ThrowIfNull(entity);
        Enter();
        try
        {
            var info = InfoFor(entity.GetType());
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.CreateVersion7(model.TimeProvider.GetUtcNow());
            var key = (info.Metadata.ClrName, entity.Id);
            if (map.TryGetValue(key, out var existing))
            {
                if (ReferenceEquals(existing.Entity, entity))
                    return entity;
                throw new ArgumentException(CoreMessages.ObjectSpaceKeyAlreadyTracked(info.Metadata.ClrName), nameof(entity));
            }
            var tracked = new Tracked(info, entity, null);
            map[key] = tracked;
            added.Add(tracked);
            return entity;
        }
        finally
        {
            Leave();
        }
    }

    public void Delete<T>(T entity) where T : SPOBaseObject
    {
        ArgumentNullException.ThrowIfNull(entity);
        Enter();
        try
        {
            var info = InfoFor(entity.GetType());
            if (!map.TryGetValue((info.Metadata.ClrName, entity.Id), out var tracked) || !ReferenceEquals(tracked.Entity, entity))
                throw new SpoObjectSpaceException(
                    SpoObjectSpaceException.NotTracked, CoreMessages.ObjectSpaceNotTracked(info.Metadata.ClrName));
            if (tracked.Snapshot is null)
            {
                map.Remove((info.Metadata.ClrName, entity.Id));
                added.Remove(tracked);
            }
            else
                tracked.PendingDelete = true;
        }
        finally
        {
            Leave();
        }
    }

    public T? Get<T>(Guid id) where T : SPOBaseObject => GetCoreAsync<T>(false, id, default).GetAwaiter().GetResult();

    public Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken) where T : SPOBaseObject
    {
        cancellationToken.ThrowIfCancellationRequested();
        return GetCoreAsync<T>(true, id, cancellationToken);
    }

    public IQueryable<T> Query<T>() where T : SPOBaseObject
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        _ = InfoFor(typeof(T));
        return new SpoQueryable<T>(this);
    }

    internal async Task<List<T>> ExecuteQueryAsync<T>(bool async, Expression expression, CancellationToken ct) where T : SPOBaseObject
    {
        Enter();
        try
        {
            var info = InfoFor(typeof(T));
            var query = QueryTranslator.Translate(expression, info);
            var rows = async ? await connection.QueryAsync(query, ct) : connection.Query(query);

            var results = new List<T>(rows.Count);
            foreach (var row in rows)
            {
                // Safe: IdColumn is the non-nullable Guid primary key column, so the value is never null.
                var id = (Guid)row[info.IdColumn]!;
                if (map.TryGetValue((info.Metadata.ClrName, id), out var existing))
                {
                    if (!existing.PendingDelete) // the tracked instance wins, with its in-memory values
                        results.Add((T)existing.Entity);
                    continue;
                }
                var entity = (SPOBaseObject)info.Metadata.CreateInstance!();
                info.WriteValues(entity, row);
                map[(info.Metadata.ClrName, id)] = new Tracked(info, entity, info.ReadValues(entity));
                results.Add((T)(object)entity);
            }
            return results;
        }
        finally
        {
            Leave();
        }
    }

    public void SaveChanges() => SaveCoreAsync(false, default).GetAwaiter().GetResult();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SaveCoreAsync(true, cancellationToken);
    }

    public void Dispose()
    {
        if (disposed)
            return;
        Enter();
        try
        {
            disposed = true;
            connection.Dispose();
        }
        finally
        {
            Interlocked.Exchange(ref busy, 0);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task<T?> GetCoreAsync<T>(bool async, Guid id, CancellationToken ct) where T : SPOBaseObject
    {
        Enter();
        try
        {
            var info = InfoFor(typeof(T));
            if (map.TryGetValue((info.Metadata.ClrName, id), out var existing))
                return existing.PendingDelete ? null : (T)existing.Entity;

            var row = async ? await connection.ReadRowAsync(info.Table, id, ct) : connection.ReadRow(info.Table, id);
            if (row is null || row[info.IsDeletedColumn] is true)
                return null;

            var entity = (SPOBaseObject)info.Metadata.CreateInstance!();
            info.WriteValues(entity, row);
            map[(info.Metadata.ClrName, id)] = new Tracked(info, entity, info.ReadValues(entity));
            return (T)(object)entity;
        }
        finally
        {
            Leave();
        }
    }

    private async Task SaveCoreAsync(bool async, CancellationToken ct)
    {
        Enter();
        var restore = new List<Action>();
        try
        {
            // Fail closed before anything is written: the identity map is keyed by the key the entity had when it was tracked.
            foreach (var tracked in map.Values)
                if (tracked.Entity.Id != tracked.Key)
                    throw new SpoObjectSpaceException(
                        SpoObjectSpaceException.KeyChanged, CoreMessages.ObjectSpaceKeyChanged(tracked.Info.Metadata.ClrName));

            var now = model.TimeProvider.GetUtcNow().UtcDateTime;
            var pending = new List<(Tracked Tracked, object?[] Values)>();

            var transaction = async ? await connection.BeginTransactionAsync(ct) : connection.BeginTransaction();
            try
            {
                foreach (var tracked in added)
                {
                    var (entity, info) = (tracked.Entity, tracked.Info);
                    var (created, modified) = (entity.CreatedAt, entity.ModifiedAt);
                    restore.Add(() => (entity.CreatedAt, entity.ModifiedAt) = (created, modified));
                    entity.CreatedAt = entity.ModifiedAt = now;

                    var values = info.ReadValues(entity);
                    if (async) await connection.InsertAsync(transaction, info.Table, values, ct);
                    else connection.Insert(transaction, info.Table, values);
                    pending.Add((tracked, values));
                }

                foreach (var tracked in map.Values.Where(t => t.Snapshot is not null))
                {
                    var (entity, info) = (tracked.Entity, tracked.Info);
                    if (!tracked.PendingDelete && !Changed(info, tracked.Snapshot!, info.ReadValues(entity)))
                        continue;

                    var (modified, isDeleted, deletedAt) = (entity.ModifiedAt, entity.IsDeleted, entity.DeletedAt);
                    restore.Add(() => (entity.ModifiedAt, entity.IsDeleted, entity.DeletedAt) = (modified, isDeleted, deletedAt));
                    entity.ModifiedAt = now;
                    if (tracked.PendingDelete)
                        (entity.IsDeleted, entity.DeletedAt) = (true, now);

                    var values = info.ReadValues(entity);
                    var changes = new List<SpoColumnValue>();
                    for (var i = 0; i < values.Length; i++)
                        if (i != info.IdColumn && !info.SameValue(i, values[i], tracked.Snapshot![i]))
                            changes.Add(new(info.Table.Columns[i].Name, values[i]));

                    var affected = async
                        ? await connection.UpdateAsync(transaction, info.Table, entity.Id, changes, ct)
                        : connection.Update(transaction, info.Table, entity.Id, changes);
                    if (affected == 0)
                        throw new SpoObjectSpaceException(
                            SpoObjectSpaceException.NoRowAffected, CoreMessages.ObjectSpaceNoRowAffected(info.Metadata.ClrName));
                    pending.Add((tracked, values));
                }

                if (async) await transaction.CommitAsync(ct);
                else transaction.Commit();
            }
            finally
            {
                // Without a commit, disposing rolls the transaction back.
                if (async) await transaction.DisposeAsync();
                else transaction.Dispose();
            }

            // Only after the commit: the saved values become the baseline. No user code runs here.
            foreach (var (tracked, values) in pending)
            {
                tracked.Snapshot = values;
                if (tracked.PendingDelete)
                    map.Remove((tracked.Info.Metadata.ClrName, tracked.Key)); // a later Get reads the row and finds it hidden
            }
            added.Clear();
        }
        catch
        {
            for (var i = restore.Count - 1; i >= 0; i--)
                restore[i]();
            throw;
        }
        finally
        {
            Leave();
        }
    }

    private static bool Changed(EntityInfo info, object?[] snapshot, object?[] current)
    {
        for (var i = 0; i < current.Length; i++)
            if (i != info.IdColumn && !info.SameValue(i, current[i], snapshot[i]))
                return true;
        return false;
    }

    private EntityInfo InfoFor(Type type)
    {
        var info = model.FindEntity(type);
        if (info is null || !info.IsUsable)
            throw new SpoObjectSpaceException(
                SpoObjectSpaceException.EntityNotInModel, CoreMessages.ObjectSpaceEntityNotInModel(type.Name));
        return info;
    }

    private void Enter()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Interlocked.CompareExchange(ref busy, 1, 0) != 0)
            throw new SpoObjectSpaceException(SpoObjectSpaceException.ConcurrentUse, CoreMessages.ObjectSpaceConcurrentUse);
    }

    private void Leave() => Interlocked.Exchange(ref busy, 0);
}
