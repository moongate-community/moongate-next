using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Data.World.Internal;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Server.Services.World;

/// <summary>
///     Sector-indexed region resolver over <see cref="IRegionDataService" />.
/// </summary>
public sealed class RegionResolverService : IRegionResolverService
{
    private readonly IRegionDataService _regionData;
    private readonly Lock _sync = new();
    private IReadOnlyDictionary<(int MapId, int SectorX, int SectorY), RegionEntry[]>? _index;

    public RegionResolverService(IRegionDataService regionData)
    {
        ArgumentNullException.ThrowIfNull(regionData);

        _regionData = regionData;
    }

    public MusicType GetMusic(int mapId, Point3D location)
    {
        return MusicTypeParser.FromName(ResolveRegion(mapId, location)?.Music);
    }

    public RegionEntry? GetRegionByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (var region in _regionData.GetAllEntries())
        {
            if (string.Equals(region.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return region;
            }
        }

        return null;
    }

    public RegionEntry? ResolveRegion(int mapId, Point3D location)
    {
        var key = RegionSectorIndex.SectorKey(mapId, location.X, location.Y);

        if (!GetIndex().TryGetValue(key, out var candidates))
        {
            return null;
        }

        foreach (var region in candidates)
        {
            foreach (var rect in region.Area)
            {
                if (rect.Contains(location.X, location.Y))
                {
                    return region;
                }
            }
        }

        return null;
    }

    private IReadOnlyDictionary<(int MapId, int SectorX, int SectorY), RegionEntry[]> GetIndex()
    {
        lock (_sync)
        {
            return _index ??= RegionSectorIndex.Build(_regionData.GetAllEntries());
        }
    }
}
