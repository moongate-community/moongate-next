using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Utils;

namespace Moongate.Server.Services.World;

/// <summary>
///     Thread-safe in-memory spatial index of live in-world entities, bucketed by map sector.
/// </summary>
public sealed class WorldSpatialIndex : IWorldSpatialIndex
{
    private readonly Dictionary<Serial, (int MapId, int SectorX, int SectorY)> _entitySector = [];
    private readonly Dictionary<Serial, MobileEntity> _mobiles = [];
    private readonly Dictionary<(int MapId, int SectorX, int SectorY), SpatialSector> _sectors = [];
    private readonly Lock _sync = new();

    public IReadOnlyCollection<MobileEntity> All
    {
        get
        {
            lock (_sync)
            {
                return _mobiles.Values.ToArray();
            }
        }
    }

    public void AddMobile(MobileEntity mobile)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        lock (_sync)
        {
            RemoveMobileUnsafe(mobile.Id);

            _mobiles[mobile.Id] = mobile;

            var key = SectorKey(mobile.MapId, mobile.Location);
            var sector = GetOrCreateSectorUnsafe(key);
            sector.Mobiles[mobile.Id] = mobile;

            if (mobile.IsPlayer)
            {
                sector.Players[mobile.Id] = mobile;
            }

            _entitySector[mobile.Id] = key;
        }
    }

    public bool TryGet(Serial id, out MobileEntity mobile)
    {
        lock (_sync)
        {
            return _mobiles.TryGetValue(id, out mobile!);
        }
    }

    public bool RemoveMobile(Serial id)
    {
        lock (_sync)
        {
            return RemoveMobileUnsafe(id);
        }
    }

    public void MoveMobile(MobileEntity mobile, Point3D newLocation)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        lock (_sync)
        {
            var newKey = SectorKey(mobile.MapId, newLocation);

            if (_entitySector.TryGetValue(mobile.Id, out var oldKey) && oldKey == newKey)
            {
                mobile.Location = newLocation;

                return;
            }

            RemoveMobileUnsafe(mobile.Id);

            mobile.Location = newLocation;
            _mobiles[mobile.Id] = mobile;

            var sector = GetOrCreateSectorUnsafe(newKey);
            sector.Mobiles[mobile.Id] = mobile;

            if (mobile.IsPlayer)
            {
                sector.Players[mobile.Id] = mobile;
            }

            _entitySector[mobile.Id] = newKey;
        }
    }

    public void AddOrUpdateItem(ItemEntity item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            RemoveItemUnsafe(item.Id);

            var key = SectorKey(item.MapId, item.Location);
            GetOrCreateSectorUnsafe(key).Items[item.Id] = item;
            _entitySector[item.Id] = key;
        }
    }

    public bool RemoveItem(Serial id)
    {
        lock (_sync)
        {
            return RemoveItemUnsafe(id);
        }
    }

    public IReadOnlyList<MobileEntity> GetMobilesInRange(int mapId, Point3D center, int range)
    {
        var results = new List<MobileEntity>();

        lock (_sync)
        {
            ForEachSectorInRange(
                mapId,
                center,
                range,
                sector =>
                {
                    foreach (var mobile in sector.Mobiles.Values)
                    {
                        if (center.InRange(mobile.Location, range))
                        {
                            results.Add(mobile);
                        }
                    }
                }
            );
        }

        return results;
    }

    public IReadOnlyList<MobileEntity> GetPlayersInRange(int mapId, Point3D center, int range)
    {
        var results = new List<MobileEntity>();

        lock (_sync)
        {
            ForEachSectorInRange(
                mapId,
                center,
                range,
                sector =>
                {
                    foreach (var player in sector.Players.Values)
                    {
                        if (center.InRange(player.Location, range))
                        {
                            results.Add(player);
                        }
                    }
                }
            );
        }

        return results;
    }

    public IReadOnlyList<ItemEntity> GetItemsInRange(int mapId, Point3D center, int range)
    {
        var results = new List<ItemEntity>();

        lock (_sync)
        {
            ForEachSectorInRange(
                mapId,
                center,
                range,
                sector =>
                {
                    foreach (var item in sector.Items.Values)
                    {
                        if (center.InRange(item.Location, range))
                        {
                            results.Add(item);
                        }
                    }
                }
            );
        }

        return results;
    }

    private bool RemoveMobileUnsafe(Serial id)
    {
        var removed = _mobiles.Remove(id);

        if (_entitySector.Remove(id, out var key) && _sectors.TryGetValue(key, out var sector))
        {
            sector.Mobiles.Remove(id);
            sector.Players.Remove(id);

            if (sector.IsEmpty)
            {
                _sectors.Remove(key);
            }
        }

        return removed;
    }

    private bool RemoveItemUnsafe(Serial id)
    {
        var removed = false;

        if (_entitySector.Remove(id, out var key) && _sectors.TryGetValue(key, out var sector))
        {
            removed = sector.Items.Remove(id);

            if (sector.IsEmpty)
            {
                _sectors.Remove(key);
            }
        }

        return removed;
    }

    private SpatialSector GetOrCreateSectorUnsafe((int MapId, int SectorX, int SectorY) key)
    {
        if (!_sectors.TryGetValue(key, out var sector))
        {
            sector = new SpatialSector();
            _sectors[key] = sector;
        }

        return sector;
    }

    private static (int MapId, int SectorX, int SectorY) SectorKey(int mapId, Point3D location)
    {
        return (mapId, location.X >> MapSectorConsts.SectorShift, location.Y >> MapSectorConsts.SectorShift);
    }

    private void ForEachSectorInRange(int mapId, Point3D center, int range, Action<SpatialSector> action)
    {
        var minSx = (center.X - range) >> MapSectorConsts.SectorShift;
        var maxSx = (center.X + range) >> MapSectorConsts.SectorShift;
        var minSy = (center.Y - range) >> MapSectorConsts.SectorShift;
        var maxSy = (center.Y + range) >> MapSectorConsts.SectorShift;

        for (var sx = minSx; sx <= maxSx; sx++)
        {
            for (var sy = minSy; sy <= maxSy; sy++)
            {
                if (_sectors.TryGetValue((mapId, sx, sy), out var sector))
                {
                    action(sector);
                }
            }
        }
    }
}
