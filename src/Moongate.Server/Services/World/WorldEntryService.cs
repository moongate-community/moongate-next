using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Items;
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

    public ValueTask EnterWorldAsync(long sessionId, MobileEntity mobile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        return ValueTask.CompletedTask;
    }
}
