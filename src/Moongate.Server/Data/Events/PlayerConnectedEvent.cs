using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Server.Data.Events;

/// <summary>
///     Tick event published when a client connects and a session is created.
/// </summary>
public sealed record PlayerConnectedEvent : ITickEvent
{
    public PlayerConnectedEvent(long sessionId, string? remoteEndPoint, DateTimeOffset at)
    {
        SessionId = sessionId;
        RemoteEndPoint = remoteEndPoint;
        At = at;
    }

    public long SessionId { get; }
    public string? RemoteEndPoint { get; }
    public DateTimeOffset At { get; }
}
