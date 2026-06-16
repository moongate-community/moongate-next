using Moongate.Abstractions.Interfaces.Services;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Network.UO.Types.Environment;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Internal.Packets;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Services.World;

/// <summary>
/// Builds and sends the world-entry packet sequence for a player entering the world.
/// </summary>
public sealed class WorldEntryService : IWorldEntryService
{
    private readonly IOutgoingPacketQueue _outgoing;
    private readonly IItemService _items;
    private readonly IContainerContentService _contents;
    private readonly IMapService _maps;
    private readonly IEventBusService _events;

    public WorldEntryService(
        IOutgoingPacketQueue outgoing,
        IItemService items,
        IContainerContentService contents,
        IMapService maps,
        IEventBusService events
    )
    {
        ArgumentNullException.ThrowIfNull(outgoing);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(contents);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(events);

        _outgoing = outgoing;
        _items = items;
        _contents = contents;
        _maps = maps;
        _events = events;
    }

    public async ValueTask EnterWorldAsync(long sessionId, MobileEntity mobile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        var map = _maps.GetMap(mobile.MapId);
        var mapWidth = map?.Width ?? 0;
        var mapHeight = map?.Height ?? 0;

        _outgoing.Enqueue(sessionId, new SupportFeaturesPacket());
        _outgoing.Enqueue(sessionId, new LoginConfirmPacket(mobile, mapWidth, mapHeight));
        _outgoing.Enqueue(sessionId, new SetMapPacket(mobile.MapId));
        _outgoing.Enqueue(sessionId, new SeasonPacket(SeasonType.Spring));
        _outgoing.Enqueue(sessionId, new DrawPlayerPacket(mobile));
        _outgoing.Enqueue(sessionId, new PlayerStatusPacket(mobile));

        foreach (var (layer, serial) in WornItemLayers.VisibleEquipped(mobile))
        {
            var item = await _items.GetByIdAsync(serial, cancellationToken);

            if (item is not null)
            {
                _outgoing.Enqueue(sessionId, new WornItemPacket(mobile, item, layer));
            }
        }

        if (mobile.BackpackId.IsValid)
        {
            var backpack = await _items.GetByIdAsync(mobile.BackpackId, cancellationToken);

            if (backpack is not null)
            {
                await _contents.EnsureContentsAsync(backpack, cancellationToken);
                _outgoing.Enqueue(sessionId, new DrawContainerPacket(backpack));

                var contents = new List<ItemEntity>(backpack.ContainedItemIds.Count);

                foreach (var contained in backpack.ContainedItemIds)
                {
                    var item = await _items.GetByIdAsync(contained, cancellationToken);

                    if (item is not null)
                    {
                        contents.Add(item);
                    }
                }

                _outgoing.Enqueue(sessionId, new ContainerContentPacket(backpack.Id, contents));
            }
        }

        _outgoing.Enqueue(sessionId, new WarModePacket());
        _outgoing.Enqueue(sessionId, new OverallLightLevelPacket(LightLevelType.Day));
        _outgoing.Enqueue(sessionId, new PersonalLightLevelPacket(mobile.Id, LightLevelType.Day));
        _outgoing.Enqueue(sessionId, new PaperdollPacket(mobile, mobile.Name ?? string.Empty));
        _outgoing.Enqueue(sessionId, new LoginCompletePacket());
        _outgoing.Enqueue(sessionId, new SetTimePacket());

        _events.Publish(new PlayerCharacterLoggedInEvent(sessionId, mobile.AccountId, mobile.Id));
    }
}
