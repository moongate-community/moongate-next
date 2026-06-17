using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x95, PacketSizing.Fixed, Length = 9, Description = "Dye Window")]
/// <summary>
/// Represents DyeWindowPacket.
/// </summary>
public class DyeWindowPacket : BaseGameNetworkPacket
{
    public DyeWindowPacket()
        : base(0x95, 9)
    {
    }

    public Serial TargetSerial { get; set; }

    public ushort Model { get; set; }

    public ushort Hue { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 8)
        {
            return false;
        }

        TargetSerial = (Serial)reader.ReadUInt32();
        Model = reader.ReadUInt16();
        Hue = reader.ReadUInt16();

        return reader.Remaining == 0;
    }
}
