using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Persistence.Data.Events;

/// <summary>
/// Async event published when a persistence snapshot starts.
/// </summary>
public sealed record SnapshotSaveStartedEvent : IAsyncEvent
{
    public DateTimeOffset At { get; }

    public SnapshotSaveStartedEvent(DateTimeOffset at)
    {
        At = at;
    }
}
