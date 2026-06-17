using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Handlers.World;

/// <summary>
/// Removes a player's live mobile from the registry when its session disconnects.
/// </summary>
public sealed class PlayerDisconnectedRegistryHandler : ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly IPlayerSessionService _playerSessions;
    private readonly IWorldMobileRegistry _registry;

    public PlayerDisconnectedRegistryHandler(IPlayerSessionService playerSessions, IWorldMobileRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(playerSessions);
        ArgumentNullException.ThrowIfNull(registry);

        _playerSessions = playerSessions;
        _registry = registry;
    }

    public void Handle(PlayerDisconnectedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (_playerSessions.TryGetBySessionId(evt.SessionId, out var session) && session.MobileSerial is { } serial)
        {
            _registry.Remove(serial);
        }
    }
}
