using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Server.Data.Events;

/// <summary>
///     Tick event published once after the host has fully started.
///     Used by diagnostic handlers to confirm the game-loop thread is alive.
/// </summary>
public sealed record ServerStartedEvent : ITickEvent
{
    public ServerStartedEvent(DateTimeOffset at)
    {
        At = at;
    }

    public DateTimeOffset At { get; }
}
