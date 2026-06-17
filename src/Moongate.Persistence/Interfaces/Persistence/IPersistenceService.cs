using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Interfaces.Ids;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
///     Owns persistence lifecycle: loads the snapshot and replays the journal at startup, autosaves
///     periodically, and hands out per-type <see cref="IDataAccess{TEntity,TKey}" /> instances.
/// </summary>
public interface IPersistenceService : IMoongateService
{
    /// <summary>Gets CRUD access with auto-increment key allocation for a registered entity type.</summary>
    IAutoDataAccess<TEntity, TKey> GetAutoDataAccess<TEntity, TKey>()
        where TKey : struct, IAutoIncrementKey<TKey>;

    /// <summary>Gets CRUD access for a registered entity type.</summary>
    IDataAccess<TEntity, TKey> GetDataAccess<TEntity, TKey>()
        where TKey : notnull;

    /// <summary>Loads the snapshot and replays the journal into memory.</summary>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Captures and writes a full snapshot, then trims the journal.</summary>
    ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default);
}
