using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Player;

/// <summary>
///     Represents a ping message packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Ping Message")]
public class PingMessagePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x73;
    private const int LengthValue = 2;

    public PingMessagePacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public PingMessagePacket(byte sequence)
        : this()
    {
        Sequence = sequence;
    }

    public byte Sequence { get; set; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Sequence = reader.ReadByte();

        return reader.Remaining == 0;
    }
}
