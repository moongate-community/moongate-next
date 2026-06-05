using Moongate.Core.Interfaces.Ids;

namespace Moongate.Persistence.Internal;

/// <summary>
/// In-memory mutable world state shared by persistence data-access instances. Not thread-safe by
/// itself; callers synchronize on <see cref="SyncRoot" />.
/// </summary>
internal sealed class PersistenceStateStore
{
    private readonly Dictionary<ushort, object> _entityBuckets = [];
    private readonly Dictionary<ushort, ulong> _lastAllocatedSeqByType = [];

    public object SyncRoot { get; } = new();

    public long LastSequenceId { get; set; }

    public void ClearBuckets()
    {
        _entityBuckets.Clear();
        _lastAllocatedSeqByType.Clear();
    }

    public Dictionary<TKey, TEntity> GetBucket<TEntity, TKey>(ushort typeId)
        where TKey : notnull
    {
        if (_entityBuckets.TryGetValue(typeId, out var existing))
        {
            return (Dictionary<TKey, TEntity>)existing;
        }

        var created = new Dictionary<TKey, TEntity>();
        _entityBuckets[typeId] = created;

        return created;
    }

    public TKey GetNextKey<TKey>(ushort typeId)
        where TKey : struct, IAutoIncrementKey<TKey>
    {
        _lastAllocatedSeqByType.TryGetValue(typeId, out var last);
        var next = last + 1;
        _lastAllocatedSeqByType[typeId] = next;

        return TKey.FromSequence(next);
    }

    public void TrackKey(ushort typeId, IAutoIncrementKey key)
    {
        if (!_lastAllocatedSeqByType.TryGetValue(typeId, out var current) || key.Sequence > current)
        {
            _lastAllocatedSeqByType[typeId] = key.Sequence;
        }
    }
}
