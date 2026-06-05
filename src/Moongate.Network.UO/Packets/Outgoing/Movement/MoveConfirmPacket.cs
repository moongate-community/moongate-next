using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Movement;

/// <summary>
/// Represents a character movement acknowledgement packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Character Move ACK/Resync Request")]
public class MoveConfirmPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x22;
    private const int LengthValue = 3;

    public byte Notoriety { get; set; }
    public byte Sequence { get; set; }

    public MoveConfirmPacket()
        : base(OpCodeValue, LengthValue) { }

    public MoveConfirmPacket(byte sequence, byte notoriety)
        : this()
    {
        Sequence = sequence;
        Notoriety = notoriety;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
        writer.Write(Notoriety);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        Sequence = reader.ReadByte();
        Notoriety = reader.ReadByte();

        return reader.Remaining == 0;
    }
}
