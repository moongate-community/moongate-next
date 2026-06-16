using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;
using Moongate.UO.Data.Utils;

namespace Moongate.Server.Services.World;

/// <summary>
/// Lazy in-memory store for teleporter definitions.
/// </summary>
public class TeleportersDataService : LazyDataService, ITeleportersDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<TeleporterEntry> _entries = [];
    private Dictionary<int, List<TeleporterEntry>> _entriesBySourceMap = [];
    private Dictionary<(int MapId, int SectorX, int SectorY), List<TeleporterEntry>> _entriesBySourceSector = [];

    public TeleportersDataService() { }

    public TeleportersDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<TeleporterEntry> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public IReadOnlyList<TeleporterEntry> GetEntriesBySourceMap(int mapId)
    {
        EnsureLoaded();

        lock (_sync)
        {
            if (!_entriesBySourceMap.TryGetValue(mapId, out var entries))
            {
                return [];
            }

            return [.. entries];
        }
    }

    public IReadOnlyList<TeleporterEntry> GetEntriesBySourceSector(int mapId, int sectorX, int sectorY)
    {
        EnsureLoaded();

        lock (_sync)
        {
            if (!_entriesBySourceSector.TryGetValue((mapId, sectorX, sectorY), out var entries))
            {
                return [];
            }

            return [.. entries];
        }
    }

    public void SetEntries(IReadOnlyList<TeleporterEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _entries = snapshot;
            _entriesBySourceMap = snapshot
                                  .GroupBy(static entry => entry.SourceMapId)
                                  .ToDictionary(
                                      static grouping => grouping.Key,
                                      static grouping => grouping.ToList()
                                  );
            _entriesBySourceSector = snapshot
                                     .GroupBy(
                                         static entry => (
                                                             entry.SourceMapId,
                                                             entry.SourceLocation.X >> MapSectorConsts.SectorShift,
                                                             entry.SourceLocation.Y >> MapSectorConsts.SectorShift
                                                         )
                                     )
                                     .ToDictionary(
                                         static grouping => grouping.Key,
                                         static grouping => grouping.ToList()
                                     );
        }

        MarkLoaded();
    }

    public bool TryGetEntryAtLocation(int mapId, Point3D location, out TeleporterEntry entry)
    {
        var sectorX = location.X >> MapSectorConsts.SectorShift;
        var sectorY = location.Y >> MapSectorConsts.SectorShift;
        var candidates = GetEntriesBySourceSector(mapId, sectorX, sectorY);

        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];

            if (candidate.SourceLocation == location)
            {
                entry = candidate;

                return true;
            }
        }

        entry = default;

        return false;
    }

    public bool TryResolveTeleportDestination(
        int mapId,
        Point3D location,
        out int destinationMapId,
        out Point3D destinationLocation,
        int maxHops = 4
    )
    {
        var currentMapId = mapId;
        var currentLocation = location;

        for (var hop = 0; hop < Math.Max(1, maxHops); hop++)
        {
            if (!TryGetEntryAtLocation(currentMapId, currentLocation, out var entry))
            {
                destinationMapId = currentMapId;
                destinationLocation = currentLocation;

                return hop > 0;
            }

            currentMapId = entry.DestinationMapId;
            currentLocation = entry.DestinationLocation;
        }

        destinationMapId = currentMapId;
        destinationLocation = currentLocation;

        return true;
    }

    protected override void LoadCore()
        => _loader?.LoadTeleporters(this);
}
