using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Outgoing "War Mode" (0x72): reports the player's combat stance (MVP: always peace).
/// </summary>
[PacketHandler(0x72, PacketSizing.Fixed, Length = 5, Description = "War Mode")]
public class WarModePacket : BaseGameNetworkPacket
{
    public MobileEntity Mobile { get; }

    public WarModePacket(MobileEntity mobile)
        : base(0x72, 5)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        Mobile = mobile;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(false);
        writer.Write((byte)0);
        writer.Write((byte)0x32);
        writer.Write((byte)0);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}
