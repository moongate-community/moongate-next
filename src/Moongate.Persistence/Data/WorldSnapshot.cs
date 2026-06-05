namespace Moongate.Persistence.Data;

/// <summary>
/// Full persisted world state stored periodically on disk.
/// </summary>
public sealed class WorldSnapshot
{
    public int Version { get; set; } = 1;
    public long CreatedUnixMilliseconds { get; set; }
    public long LastSequenceId { get; set; }
    public EntitySnapshotBucket[] EntityBuckets { get; set; } = [];
}
