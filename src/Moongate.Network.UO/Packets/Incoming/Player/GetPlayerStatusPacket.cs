using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.Network.UO.Types.Player;

namespace Moongate.Network.UO.Packets.Incoming.Player;

/// <summary>
///     Incoming "Get Player Status" (0x34): the client requests a mobile's status bar (0x04) or skills (0x05).
/// </summary>
[PacketHandler(0x34, PacketSizing.Fixed, Length = 10, Description = "Get Player Status")]
public class GetPlayerStatusPacket : BaseGameNetworkPacket
{
    public GetPlayerStatusPacket()
        : base(0x34, 10)
    {
    }

    public uint UnknownPattern { get; set; }
    public GetPlayerStatusType StatusType { get; set; }
    public uint MobileSerial { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 9)
        {
            return false;
        }

        UnknownPattern = reader.ReadUInt32();
        StatusType = (GetPlayerStatusType)reader.ReadByte();
        MobileSerial = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
