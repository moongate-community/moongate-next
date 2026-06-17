using Moongate.Core.Interfaces.Ids;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
///     Extends <see cref="IDataAccess{TEntity,TKey}" /> for entities keyed by an
///     <see cref="IAutoIncrementKey{TSelf}" />, adding auto-increment ID allocation that survives restarts.
/// </summary>
public interface IAutoDataAccess<TEntity, TKey> : IDataAccess<TEntity, TKey>
    where TKey : struct, IAutoIncrementKey<TKey>
{
    /// <summary>
    ///     Allocates and returns the next available key for this entity type.
    ///     The counter is reconstructed from the maximum stored key on every boot.
    /// </summary>
    ValueTask<TKey> NextIdAsync(CancellationToken cancellationToken = default);
}
