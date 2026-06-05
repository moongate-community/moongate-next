using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Server.Data.Events;

/// <summary>
/// Tick event published when a client disconnects and its session is removed.
/// </summary>
public sealed record PlayerDisconnectedEvent : ITickEvent
{
    public long SessionId { get; }
    public string? RemoteEndPoint { get; }
    public DateTimeOffset At { get; }

    public PlayerDisconnectedEvent(long sessionId, string? remoteEndPoint, DateTimeOffset at)
    {
        SessionId = sessionId;
        RemoteEndPoint = remoteEndPoint;
        At = at;
    }
}
