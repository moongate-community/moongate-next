using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Data;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>Outgoing "Mobile Incoming" (0x78): draws another mobile (with equipped items) for the client.</summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Mobile Incoming")]
public class MobileIncomingPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x78;

    public MobileIncomingPacket(MobileEntity mobile, IReadOnlyList<(ItemLayerType Layer, ItemEntity Item)> equipped)
        : base(OpCodeValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(equipped);

        Mobile = mobile;
        Equipped = equipped;
    }

    public MobileEntity Mobile { get; }
    public IReadOnlyList<(ItemLayerType Layer, ItemEntity Item)> Equipped { get; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)0); // length placeholder

        writer.Write(Mobile.Id.Value);
        writer.Write((short)Mobile.BodyId);
        writer.Write((ushort)Mobile.Location.X);
        writer.Write((ushort)Mobile.Location.Y);
        writer.Write((sbyte)Mobile.Location.Z);
        writer.Write((byte)Mobile.Direction);
        writer.Write(Mobile.SkinHue.Value);
        writer.Write(MobilePacketFlags.For(Mobile));
        writer.Write((byte)Mobile.Notoriety);

        foreach (var (layer, item) in Equipped)
        {
            var hue = item.Hue.Value;
            var graphic = (ushort)item.ItemId;

            if (hue != 0)
            {
                graphic |= 0x8000;
            }

            writer.Write(item.Id.Value);
            writer.Write(graphic);
            writer.Write((byte)layer);

            if (hue != 0)
            {
                writer.Write(hue);
            }
        }

        writer.Write((uint)0); // terminator
        writer.WritePacketLength();
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
