using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Server.Data.Network;

/// <summary>
/// Packet queued for outbound delivery to a connected game session.
/// </summary>
public sealed record OutgoingPacketEnvelope
{
    public long SessionId { get; }
    public IGameNetworkPacket Packet { get; }
    public DateTimeOffset EnqueuedAt { get; }

    public OutgoingPacketEnvelope(long sessionId, IGameNetworkPacket packet, DateTimeOffset enqueuedAt)
    {
        ArgumentNullException.ThrowIfNull(packet);

        SessionId = sessionId;
        Packet = packet;
        EnqueuedAt = enqueuedAt;
    }
}
