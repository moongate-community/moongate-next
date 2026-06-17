using Moongate.UO.Data.Utils;

namespace Moongate.Server.Data.World.Internal;

/// <summary>
///     Builds a sector index for fast region lookups: each region is bucketed into every
///     (map, sectorX, sectorY) its rectangles overlap, and buckets are ordered by priority desc.
/// </summary>
public static class RegionSectorIndex
{
    public static IReadOnlyDictionary<(int MapId, int SectorX, int SectorY), RegionEntry[]> Build(
        IReadOnlyList<RegionEntry> regions
    )
    {
        ArgumentNullException.ThrowIfNull(regions);

        var buckets = new Dictionary<(int, int, int), List<RegionEntry>>();
        var shift = MapSectorConsts.SectorShift;

        foreach (var region in regions)
        {
            var sectors = new HashSet<(int SectorX, int SectorY)>();

            foreach (var rect in region.Area)
            {
                var minSx = Math.Min(rect.X1, rect.X2) >> shift;
                var maxSx = Math.Max(rect.X1, rect.X2) >> shift;
                var minSy = Math.Min(rect.Y1, rect.Y2) >> shift;
                var maxSy = Math.Max(rect.Y1, rect.Y2) >> shift;

                for (var sx = minSx; sx <= maxSx; sx++)
                {
                    for (var sy = minSy; sy <= maxSy; sy++)
                    {
                        sectors.Add((sx, sy));
                    }
                }
            }

            foreach (var (sx, sy) in sectors)
            {
                var key = (region.MapId, sx, sy);

                if (!buckets.TryGetValue(key, out var list))
                {
                    list = [];
                    buckets[key] = list;
                }

                list.Add(region);
            }
        }

        return buckets.ToDictionary(
            static pair => pair.Key,
            static pair => pair.Value.OrderByDescending(static region => region.Priority).ToArray()
        );
    }

    /// <summary>Returns the sector bucket key for a tile coordinate on a map.</summary>
    public static (int MapId, int SectorX, int SectorY) SectorKey(int mapId, int x, int y)
    {
        return (mapId, x >> MapSectorConsts.SectorShift, y >> MapSectorConsts.SectorShift);
    }
}
