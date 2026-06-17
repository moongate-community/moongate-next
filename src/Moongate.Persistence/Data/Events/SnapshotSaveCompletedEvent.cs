using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Persistence.Data.Events;

/// <summary>
///     Async event published after a persistence snapshot completes successfully.
/// </summary>
public sealed record SnapshotSaveCompletedEvent : IAsyncEvent
{
    public SnapshotSaveCompletedEvent(
        long lastSequenceId,
        int entityBucketCount,
        DateTimeOffset startedAt,
        DateTimeOffset at
    )
    {
        LastSequenceId = lastSequenceId;
        EntityBucketCount = entityBucketCount;
        StartedAt = startedAt;
        At = at;
    }

    public long LastSequenceId { get; }
    public int EntityBucketCount { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset At { get; }
}
