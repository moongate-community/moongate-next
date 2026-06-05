using Moongate.Persistence.Data;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Reads and writes complete world snapshots.
/// </summary>
public interface ISnapshotService
{
    /// <summary>Loads the latest snapshot, or null when none exists.</summary>
    ValueTask<WorldSnapshot?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves a full world snapshot atomically.</summary>
    ValueTask SaveAsync(WorldSnapshot snapshot, CancellationToken cancellationToken = default);
}
