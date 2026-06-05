using Moongate.Abstractions.Data.Network;

namespace Moongate.Abstractions.Interfaces.Network;

/// <summary>
/// Handles a parsed inbound game packet on the game-loop packet dispatch path.
/// </summary>
/// <typeparam name="TPacket">Concrete inbound packet type.</typeparam>
/// <remarks>
/// Packet handlers run on the game-loop thread through the tick event bus. Implementations should
/// finish quickly and avoid blocking I/O.
/// </remarks>
public interface IPacketHandler<TPacket>
    where TPacket : IGameNetworkPacket
{
    /// <summary>
    /// Handles the packet.
    /// </summary>
    /// <param name="context">Packet context with session metadata and outbound helpers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(PacketContext<TPacket> context, CancellationToken cancellationToken = default);
}
