using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Handlers.World;

/// <summary>
///     Removes a player's live mobile from the registry when its session disconnects.
/// </summary>
public sealed class PlayerDisconnectedRegistryHandler : ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly IWorldSpatialIndex _index;
    private readonly IPlayerSessionService _playerSessions;

    public PlayerDisconnectedRegistryHandler(IPlayerSessionService playerSessions, IWorldSpatialIndex index)
    {
        ArgumentNullException.ThrowIfNull(playerSessions);
        ArgumentNullException.ThrowIfNull(index);

        _playerSessions = playerSessions;
        _index = index;
    }

    public void Handle(PlayerDisconnectedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (_playerSessions.TryGetBySessionId(evt.SessionId, out var session) && session.MobileSerial is { } serial)
        {
            _index.RemoveMobile(serial);
        }
    }
}
