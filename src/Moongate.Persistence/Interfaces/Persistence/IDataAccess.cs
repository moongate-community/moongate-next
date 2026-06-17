namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
///     CRUD access to a registered persisted entity type. Reads complete synchronously from memory;
///     writes append to the journal.
/// </summary>
public interface IDataAccess<TEntity, in TKey>
{
    /// <summary>Returns the current number of persisted entities.</summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all persisted entities (detached clones).</summary>
    ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets an entity by id, or null when absent (detached clone).</summary>
    ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Returns a queryable snapshot of persisted entities (detached clones).</summary>
    IQueryable<TEntity> Query();

    /// <summary>Removes an entity by id; returns true when one was removed.</summary>
    ValueTask<bool> RemoveAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>Inserts or updates an entity.</summary>
    ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
