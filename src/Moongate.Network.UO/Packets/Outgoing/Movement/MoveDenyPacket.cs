using Moongate.Core.Types;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Movement;

/// <summary>
/// Represents a character movement rejection packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Character Move Rejection")]
public class MoveDenyPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x21;
    private const int LengthValue = 8;

    public DirectionType Direction { get; set; }
    public byte Sequence { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public sbyte Z { get; set; }

    public MoveDenyPacket()
        : base(OpCodeValue, LengthValue) { }

    public MoveDenyPacket(byte sequence, short x, short y, DirectionType direction, sbyte z)
        : this()
    {
        Sequence = sequence;
        X = x;
        Y = y;
        Direction = direction;
        Z = z;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
        writer.Write(X);
        writer.Write(Y);
        writer.Write((byte)Direction);
        writer.Write(Z);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 7)
        {
            return false;
        }

        Sequence = reader.ReadByte();
        X = reader.ReadInt16();
        Y = reader.ReadInt16();
        Direction = (DirectionType)reader.ReadByte();
        Z = reader.ReadSByte();

        return reader.Remaining == 0;
    }
}
