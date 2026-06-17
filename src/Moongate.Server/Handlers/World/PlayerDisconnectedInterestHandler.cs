using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Handlers.World;

/// <summary>
/// On disconnect, deletes the leaving player's mobile from every observer that knew it
/// and clears the leaver's own known-set.
/// </summary>
public sealed class PlayerDisconnectedInterestHandler : ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerSessionService _sessions;
    private readonly IInterestManagementService _interest;

    public PlayerDisconnectedInterestHandler(IPlayerSessionService sessions, IInterestManagementService interest)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(interest);

        _sessions = sessions;
        _interest = interest;
    }

    public void Handle(PlayerDisconnectedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (_sessions.TryGetBySessionId(evt.SessionId, out var session) && session.MobileSerial is { } serial)
        {
            _interest.OnEntityRemoved(serial);
        }

        _interest.ForgetSession(evt.SessionId);
    }
}
