using Moongate.Core.Geometry;
using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Provides access to teleporter definitions loaded from server asset data.
/// </summary>
public interface ITeleportersDataService : IDataService
{
    /// <summary>
    /// Returns all loaded teleporter definitions.
    /// </summary>
    /// <returns>All teleporter definitions.</returns>
    IReadOnlyList<TeleporterEntry> GetAllEntries();

    /// <summary>
    /// Returns teleporter definitions filtered by source map id.
    /// </summary>
    /// <param name="mapId">Source map id.</param>
    /// <returns>Teleporter definitions for the requested source map.</returns>
    IReadOnlyList<TeleporterEntry> GetEntriesBySourceMap(int mapId);

    /// <summary>
    /// Returns teleporter definitions filtered by source map sector.
    /// </summary>
    /// <param name="mapId">Source map id.</param>
    /// <param name="sectorX">Sector X.</param>
    /// <param name="sectorY">Sector Y.</param>
    /// <returns>Teleporter definitions in the requested source sector.</returns>
    IReadOnlyList<TeleporterEntry> GetEntriesBySourceSector(int mapId, int sectorX, int sectorY);

    /// <summary>
    /// Replaces all currently loaded teleporter definitions.
    /// </summary>
    /// <param name="entries">Teleporter definitions.</param>
    void SetEntries(IReadOnlyList<TeleporterEntry> entries);

    /// <summary>
    /// Tries to resolve an exact source-location teleporter entry.
    /// </summary>
    /// <param name="mapId">Source map id.</param>
    /// <param name="location">Source location.</param>
    /// <param name="entry">Resolved teleporter entry when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGetEntryAtLocation(int mapId, Point3D location, out TeleporterEntry entry);

    /// <summary>
    /// Tries to resolve a teleporter destination, following chained teleporters up to a hop limit.
    /// </summary>
    /// <param name="mapId">Source map id.</param>
    /// <param name="location">Source location.</param>
    /// <param name="destinationMapId">Resolved destination map id.</param>
    /// <param name="destinationLocation">Resolved destination location.</param>
    /// <param name="maxHops">Maximum chained teleporter hops.</param>
    /// <returns><c>true</c> when a destination was resolved; otherwise <c>false</c>.</returns>
    bool TryResolveTeleportDestination(
        int mapId,
        Point3D location,
        out int destinationMapId,
        out Point3D destinationLocation,
        int maxHops = 4
    );
}
