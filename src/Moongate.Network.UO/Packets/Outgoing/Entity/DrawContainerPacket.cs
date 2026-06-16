using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>
/// Outgoing "Draw Container" (0x24): opens a container gump on the client.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Draw Container")]
public class DrawContainerPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x24;
    private const int LengthValue = 9;
    private const int DefaultBackpackGumpId = 0x3C;
    private const short ContainerDisplayFlag = 0x7D; // UO protocol trailer (standard container value)

    public ItemEntity Container { get; }

    public DrawContainerPacket(ItemEntity container)
        : base(OpCodeValue, LengthValue)
    {
        ArgumentNullException.ThrowIfNull(container);

        Container = container;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Container.Id.Value);
        writer.Write((ushort)(Container.GumpId ?? DefaultBackpackGumpId));
        writer.Write(ContainerDisplayFlag);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}
