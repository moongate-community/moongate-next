using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x08, PacketSizing.Fixed, Length = 14, Description = "Drop Item")]

/// <summary>
/// Represents DropItemPacket.
/// </summary>
public class DropItemPacket : BaseGameNetworkPacket
{
    public Serial DestinationSerial { get; set; }
    public bool IsGroundDrop => DestinationSerial == Serial.MinusOne;

    public Serial ItemSerial { get; set; }

    public Point3D Location { get; set; }

    public DropItemPacket()
        : base(0x08) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining is not (13 or 14))
        {
            return false;
        }

        ItemSerial = (Serial)reader.ReadUInt32();
        var x = reader.ReadInt16();
        var y = reader.ReadInt16();
        var z = reader.ReadSByte();

        if (reader.Remaining == 5)
        {
            _ = reader.ReadByte();
        }

        DestinationSerial = (Serial)reader.ReadUInt32();
        Location = new(x, y, z);

        return reader.Remaining == 0;
    }
}
