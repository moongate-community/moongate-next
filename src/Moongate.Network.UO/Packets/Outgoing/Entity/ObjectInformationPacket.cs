using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>Outgoing "Object Information" (0xF3): draws a ground item for the client.</summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Object Information")]
public class ObjectInformationPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xF3;
    private const int LengthValue = 26;

    public ObjectInformationPacket(ItemEntity item)
        : base(OpCodeValue, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(item);

        Item = item;
    }

    public ItemEntity Item { get; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)0x0001);          // command
        writer.Write((byte)0);                 // data type (0 = item)
        writer.Write(Item.Id.Value);           // serial
        writer.Write((ushort)Item.ItemId);     // graphic
        writer.Write((byte)0);                 // facing
        writer.Write((ushort)Item.Amount);     // amount (first)
        writer.Write((ushort)0);               // amount (second)
        writer.Write((ushort)Item.Location.X); // x
        writer.Write((ushort)Item.Location.Y); // y
        writer.Write((sbyte)Item.Location.Z);  // z
        writer.Write((byte)0);                 // layer
        writer.Write(Item.Hue.Value);          // color
        writer.Write((byte)0);                 // flags
        writer.Write((ushort)0);               // unknown (HS)
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
