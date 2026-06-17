using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Network;

/// <summary>
///     Base class for typed packet handlers that need common Moongate services.
/// </summary>
/// <typeparam name="TPacket">Concrete inbound packet type.</typeparam>
public abstract class PacketHandlerBase<TPacket> : IPacketHandler<TPacket>
    where TPacket : IGameNetworkPacket
{
    protected PacketHandlerBase(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions
    )
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(playerSessions);

        EventBus = eventBus;
        Sessions = sessions;
        PlayerSessions = playerSessions;
    }

    /// <summary>
    ///     Event bus available to packet handlers for publishing domain events.
    /// </summary>
    protected IEventBusService EventBus { get; }

    /// <summary>
    ///     Active session manager available to packet handlers.
    /// </summary>
    protected INetworkSessionManager Sessions { get; }

    protected IPlayerSessionService PlayerSessions { get; }

    public abstract Task HandleAsync(
        PacketContext<TPacket> context,
        CancellationToken cancellationToken = default
    );
}
