using Moongate.UO.Data.Maps;

namespace Moongate.UO.Data.Interfaces.Maps;

/// <summary>
/// Provides access to the registered UO map facets and their static geography.
/// </summary>
public interface IMapService
{
    /// <summary>All registered facets.</summary>
    IReadOnlyList<Map> Maps { get; }

    /// <summary>Returns the facet with the given map id, or <c>null</c> when none is registered.</summary>
    /// <param name="mapId">Map id (0 = Felucca, 1 = Trammel, ...).</param>
    Map? GetMap(int mapId);
}
