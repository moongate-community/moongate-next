using Moongate.Core.Interfaces.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Internal;
using Moongate.Persistence.Types;
using ZLinq;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// In-memory <see cref="IDataAccess{TEntity,TKey}" /> backed by a shared state store; every mutation
/// is appended to the journal. Reads return detached clones for snapshot isolation.
/// </summary>
public class GenericDataAccess<TEntity, TKey> : IDataAccess<TEntity, TKey>
    where TKey : notnull
{
    private readonly PersistenceStateStore _stateStore;
    private readonly IJournalService _journalService;
    private readonly IPersistenceEntityDescriptor<TEntity, TKey> _descriptor;

    internal GenericDataAccess(
        PersistenceStateStore stateStore,
        IJournalService journalService,
        IPersistenceEntityDescriptor<TEntity, TKey> descriptor
    )
    {
        _stateStore = stateStore;
        _journalService = journalService;
        _descriptor = descriptor;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(Bucket().Count);
        }
    }

    public ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateStore.SyncRoot)
        {
            IReadOnlyCollection<TEntity> clones = Bucket()
                                                  .Values
                                                  .AsValueEnumerable()
                                                  .Select(_descriptor.Clone)
                                                  .ToArray();

            return ValueTask.FromResult(clones);
        }
    }

    public ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(Bucket().TryGetValue(id, out var entity) ? _descriptor.Clone(entity) : default);
        }
    }

    public IQueryable<TEntity> Query()
    {
        lock (_stateStore.SyncRoot)
        {
            var clones = Bucket()
                         .Values
                         .AsValueEnumerable()
                         .Select(_descriptor.Clone)
                         .ToArray();

            return clones.AsQueryable();
        }
    }

    public async ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default)
    {
        JournalEntry? entry = null;

        lock (_stateStore.SyncRoot)
        {
            if (Bucket().Remove(id))
            {
                entry = new()
                {
                    SequenceId = ++_stateStore.LastSequenceId,
                    TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TypeId = _descriptor.TypeId,
                    Operation = JournalEntityOperationType.Remove,
                    Payload = _descriptor.SerializeKey(id)
                };
            }
        }

        if (entry is null)
        {
            return false;
        }

        await _journalService.AppendAsync(entry, cancellationToken);

        return true;
    }

    public async ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        JournalEntry entry;

        lock (_stateStore.SyncRoot)
        {
            var clone = _descriptor.Clone(entity);
            var key = _descriptor.GetKey(clone);
            Bucket()[key] = clone;

            if (key is IAutoIncrementKey autoKey)
            {
                _stateStore.TrackKey(_descriptor.TypeId, autoKey);
            }

            entry = new()
            {
                SequenceId = ++_stateStore.LastSequenceId,
                TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                TypeId = _descriptor.TypeId,
                Operation = JournalEntityOperationType.Upsert,
                Payload = _descriptor.SerializeEntity(clone)
            };
        }

        await _journalService.AppendAsync(entry, cancellationToken);
    }

    private protected Dictionary<TKey, TEntity> Bucket()
        => _stateStore.GetBucket<TEntity, TKey>(_descriptor.TypeId);
}
