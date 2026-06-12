using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Packets.Incoming.Interaction;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Handlers.Items;

[RegisterPacketHandler]
public sealed class ContainerDoubleClickPacketHandler : IPacketHandler<DoubleClickPacket>
{
    private readonly IItemService _items;
    private readonly IContainerContentService _contents;

    public ContainerDoubleClickPacketHandler(IItemService items, IContainerContentService contents)
    {

        _items = items;
        _contents = contents;
    }

    public async Task HandleAsync(
        PacketContext<DoubleClickPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (context.Packet.TargetSerial.IsItem)
        {
            var item = await _items.GetByIdAsync(context.Packet.TargetSerial, cancellationToken);

            if (item is null)
            {
                return;
            }

            await _contents.EnsureContentsAsync(item, cancellationToken);
        }
    }
}
