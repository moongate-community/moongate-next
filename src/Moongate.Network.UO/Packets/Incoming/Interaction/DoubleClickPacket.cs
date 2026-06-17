using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x06, PacketSizing.Fixed, Length = 5, Description = "Double Click")]
/// <summary>
/// Represents DoubleClickPacket.
/// </summary>
public class DoubleClickPacket : BaseGameNetworkPacket
{
    public DoubleClickPacket()
        : base(0x06, 5)
    {
    }

    public Serial TargetSerial { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 4)
        {
            return false;
        }

        TargetSerial = (Serial)reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
