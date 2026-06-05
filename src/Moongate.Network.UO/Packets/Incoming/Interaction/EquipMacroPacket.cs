using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xEC, PacketSizing.Variable, Description = "Equip Macro (KR)")]

/// <summary>
/// Represents EquipMacroPacket.
/// </summary>
public class EquipMacroPacket : BaseGameNetworkPacket
{
    public EquipMacroPacket()
        : base(0xEC) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
