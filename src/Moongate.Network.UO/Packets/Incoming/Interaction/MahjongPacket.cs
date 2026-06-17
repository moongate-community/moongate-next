using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xDA, PacketSizing.Variable, Description = "Mahjong")]
/// <summary>
/// Represents MahjongPacket.
/// </summary>
public class MahjongPacket : BaseGameNetworkPacket
{
    public MahjongPacket()
        : base(0xDA)
    {
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
