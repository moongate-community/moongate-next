using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.World;

/// <summary>
///     Populates the in-memory spatial index from persisted entities at boot: NPC mobiles
///     (non-player) and ground items (not in a container, not equipped). Player characters are
///     added on login, and contained/equipped items are not world-positioned, so both are skipped.
/// </summary>
public sealed class WorldEntitiesBootService : IMoongateService
{
    private readonly IWorldSpatialIndex _index;
    private readonly IAutoDataAccess<ItemEntity, Serial> _items;
    private readonly ILogger _logger = Log.ForContext<WorldEntitiesBootService>();
    private readonly IAutoDataAccess<MobileEntity, Serial> _mobiles;

    public WorldEntitiesBootService(
        IAutoDataAccess<MobileEntity, Serial> mobiles,
        IAutoDataAccess<ItemEntity, Serial> items,
        IWorldSpatialIndex index
    )
    {
        ArgumentNullException.ThrowIfNull(mobiles);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(index);

        _mobiles = mobiles;
        _items = items;
        _index = index;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var npcCount = 0;

        foreach (var mobile in await _mobiles.GetAllAsync(cancellationToken))
        {
            if (!mobile.IsPlayer)
            {
                _index.AddMobile(mobile);
                npcCount++;
            }
        }

        var itemCount = 0;

        foreach (var item in await _items.GetAllAsync(cancellationToken))
        {
            if (item.ParentContainerId == Serial.Zero && item.EquippedMobileId == Serial.Zero)
            {
                _index.AddOrUpdateItem(item);
                itemCount++;
            }
        }

        _logger.Information(
            "Spatial index cold-start: {NpcCount} NPC(s), {ItemCount} ground item(s) loaded",
            npcCount,
            itemCount
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
