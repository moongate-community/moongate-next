using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Network;

/// <summary>
/// Base class for typed packet handlers that need common Moongate services.
/// </summary>
/// <typeparam name="TPacket">Concrete inbound packet type.</typeparam>
public abstract class PacketHandlerBase<TPacket> : IPacketHandler<TPacket>
    where TPacket : IGameNetworkPacket
{
    /// <summary>
    /// Event bus available to packet handlers for publishing domain events.
    /// </summary>
    protected IEventBusService EventBus { get; }

    /// <summary>
    /// Active session manager available to packet handlers.
    /// </summary>
    protected INetworkSessionManager Sessions { get; }

    protected PacketHandlerBase(IEventBusService eventBus, INetworkSessionManager sessions)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(sessions);

        EventBus = eventBus;
        Sessions = sessions;
    }

    public abstract Task HandleAsync(
        PacketContext<TPacket> context,
        CancellationToken cancellationToken = default
    );
}
