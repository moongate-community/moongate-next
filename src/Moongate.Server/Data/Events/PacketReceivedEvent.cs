using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Server.Data.Events;

/// <summary>
/// Tick event published for every successfully parsed inbound packet. Handlers run on the
/// game-loop thread via <see cref="ITickEvent" /> dispatch.
/// </summary>
public sealed record PacketReceivedEvent : ITickEvent
{
    public long SessionId { get; }
    public byte OpCode { get; }
    public IGameNetworkPacket Packet { get; }
    public DateTimeOffset At { get; }

    public PacketReceivedEvent(long sessionId, byte opCode, IGameNetworkPacket packet, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(packet);

        SessionId = sessionId;
        OpCode = opCode;
        Packet = packet;
        At = at;
    }
}
