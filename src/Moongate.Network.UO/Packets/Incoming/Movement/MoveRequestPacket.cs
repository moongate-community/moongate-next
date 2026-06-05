using Moongate.Core.Types;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Movement;

/// <summary>
/// Represents a movement request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Move Request")]
public class MoveRequestPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x02;
    private const int LengthValue = 7;

    public DirectionType Direction { get; set; }
    public DirectionType WalkDirection => (DirectionType)((byte)Direction & 0x07);
    public bool IsRunning => (Direction & DirectionType.Running) != 0;
    public byte Sequence { get; set; }
    public uint FastWalkKey { get; set; }

    public MoveRequestPacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 6)
        {
            return false;
        }

        Direction = (DirectionType)reader.ReadByte();
        Sequence = reader.ReadByte();
        FastWalkKey = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
