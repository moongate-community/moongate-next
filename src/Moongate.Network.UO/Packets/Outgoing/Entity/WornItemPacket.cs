using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
///     Outgoing "Worn Item" (0x2E): equips a single visible item on a mobile's figure/paperdoll.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Worn Item")]
public class WornItemPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x2E;
    private const int LengthValue = 15;

    public WornItemPacket(MobileEntity mobile, ItemEntity item, ItemLayerType layer)
        : base(OpCodeValue, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(item);

        Mobile = mobile;
        Item = item;
        Layer = layer;
    }

    public MobileEntity Mobile { get; }
    public ItemEntity Item { get; }
    public ItemLayerType Layer { get; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Item.Id.Value);
        writer.Write((ushort)Item.ItemId);
        writer.Write((byte)0);
        writer.Write((byte)Layer);
        writer.Write(Mobile.Id.Value);
        writer.Write(Item.Hue.Value);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
