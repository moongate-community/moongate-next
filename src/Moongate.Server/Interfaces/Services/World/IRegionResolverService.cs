using Moongate.Core.Geometry;
using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Resolves the region that applies at a map location, plus region-derived lookups.
/// </summary>
public interface IRegionResolverService
{
    /// <summary>Returns the highest-priority region whose area contains the point, or null.</summary>
    RegionEntry? ResolveRegion(int mapId, Point3D location);

    /// <summary>Returns the resolved region's music, or an empty string when none applies.</summary>
    string GetMusic(int mapId, Point3D location);

    /// <summary>Returns the first region with the given name (case-insensitive), or null.</summary>
    RegionEntry? GetRegionByName(string name);
}
