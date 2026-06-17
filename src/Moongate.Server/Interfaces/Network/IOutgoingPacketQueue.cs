using Moongate.Abstractions.Interfaces.Network;
using Moongate.Server.Data.Network;

namespace Moongate.Server.Interfaces.Network;

/// <summary>
///     Thread-safe outbound packet queue used by gameplay systems to request network sends by session id.
/// </summary>
public interface IOutgoingPacketQueue
{
    /// <summary>
    ///     Number of currently queued outbound packets.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Clears all queued packets.
    /// </summary>
    /// <param name="handler">Optional handler invoked for each cleared packet.</param>
    /// <returns>Number of packets cleared.</returns>
    int Clear(Action<OutgoingPacketEnvelope>? handler = null);

    /// <summary>
    ///     Drains queued packets in FIFO order.
    /// </summary>
    /// <param name="maxItems">Maximum number of packets to drain.</param>
    /// <param name="handler">Handler invoked for each drained packet.</param>
    /// <returns>Number of packets drained.</returns>
    int Drain(int maxItems, Func<OutgoingPacketEnvelope, bool> handler);

    /// <summary>
    ///     Enqueues a packet for the target session.
    /// </summary>
    /// <typeparam name="TPacket">Packet type.</typeparam>
    /// <param name="sessionId">Target session id.</param>
    /// <param name="packet">Packet to send.</param>
    void Enqueue<TPacket>(long sessionId, TPacket packet)
        where TPacket : IGameNetworkPacket;
}
