using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
///     Outgoing "Add Multiple Items To Container" (0x3C): sends a container's contents in one packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Add Multiple Items To Container")]
public class ContainerContentPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x3C;
    private const int HeaderLength = 5;
    private const int EntryLength = 20;

    public ContainerContentPacket(Serial containerId, IReadOnlyList<ItemEntity> items)
        : base(OpCodeValue)
    {
        ArgumentNullException.ThrowIfNull(items);

        ContainerId = containerId;
        Items = items;
    }

    public Serial ContainerId { get; }
    public IReadOnlyList<ItemEntity> Items { get; }

    public override void Write(ref SpanWriter writer)
    {
        var total = Items.Count;

        writer.Write(OpCode);
        writer.Write((ushort)(HeaderLength + total * EntryLength));
        writer.Write((ushort)total);

        for (var i = 0; i < total; i++)
        {
            var item = Items[i];

            writer.Write(item.Id.Value);
            writer.Write((ushort)item.ItemId);
            writer.Write((byte)0);
            writer.Write((short)Math.Clamp(item.Amount, 1, short.MaxValue));
            writer.Write((short)item.ContainerPosition.X);
            writer.Write((short)item.ContainerPosition.Y);
            writer.Write((byte)i);
            writer.Write(ContainerId.Value);
            writer.Write((short)item.Hue.Value);
        }
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return false;
    }
}
