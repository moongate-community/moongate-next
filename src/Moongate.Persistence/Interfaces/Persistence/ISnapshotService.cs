using Moongate.Persistence.Data;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>Reads and writes per-type entity snapshot files.</summary>
public interface ISnapshotService
{
    /// <summary>Deletes a type's snapshot file, if present, so an emptied type does not resurrect on reload.</summary>
    ValueTask DeleteBucketAsync(string typeName, CancellationToken cancellationToken = default);

    /// <summary>Loads a type's snapshot bucket by type name, or null when absent or unreadable.</summary>
    ValueTask<PersistedBucket?> LoadBucketAsync(string typeName, CancellationToken cancellationToken = default);

    /// <summary>Saves a single type's bucket to its own file atomically.</summary>
    ValueTask SaveBucketAsync(
        EntitySnapshotBucket bucket,
        long lastSequenceId,
        CancellationToken cancellationToken = default
    );
}
