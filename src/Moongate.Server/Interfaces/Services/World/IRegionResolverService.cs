using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Resolves the region that applies at a map location, plus region-derived lookups.
/// </summary>
public interface IRegionResolverService
{
    /// <summary>Returns the resolved region's music, or <see cref="MusicType.NoMusic" /> when none applies.</summary>
    MusicType GetMusic(int mapId, Point3D location);

    /// <summary>Returns the first region with the given name (case-insensitive), or null.</summary>
    RegionEntry? GetRegionByName(string name);

    /// <summary>Returns the highest-priority region whose area contains the point, or null.</summary>
    RegionEntry? ResolveRegion(int mapId, Point3D location);
}
