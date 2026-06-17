using Moongate.Core.Interfaces.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Internal;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
///     <see cref="IAutoDataAccess{TEntity,TKey}" /> implementation: extends <see cref="GenericDataAccess{TEntity,TKey}" />
///     with auto-increment key allocation backed by <see cref="PersistenceStateStore" />.
/// </summary>
internal sealed class AutoDataAccess<TEntity, TKey> : GenericDataAccess<TEntity, TKey>, IAutoDataAccess<TEntity, TKey>
    where TKey : struct, IAutoIncrementKey<TKey>
{
    private readonly IPersistenceEntityDescriptor<TEntity, TKey> _descriptor;
    private readonly PersistenceStateStore _stateStore;

    internal AutoDataAccess(
        PersistenceStateStore stateStore,
        IJournalService journalService,
        IPersistenceEntityDescriptor<TEntity, TKey> descriptor
    ) : base(stateStore, journalService, descriptor)
    {
        _stateStore = stateStore;
        _descriptor = descriptor;
    }

    public ValueTask<TKey> NextIdAsync(CancellationToken cancellationToken = default)
    {
        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.GetNextKey<TKey>(_descriptor.TypeId));
        }
    }
}
