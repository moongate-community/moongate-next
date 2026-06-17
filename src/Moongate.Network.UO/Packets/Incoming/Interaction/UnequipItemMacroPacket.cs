using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xED, PacketSizing.Variable, Description = "Unequip Item Macro (KR)")]
/// <summary>
/// Represents UnequipItemMacroPacket.
/// </summary>
public class UnequipItemMacroPacket : BaseGameNetworkPacket
{
    public UnequipItemMacroPacket()
        : base(0xED)
    {
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
