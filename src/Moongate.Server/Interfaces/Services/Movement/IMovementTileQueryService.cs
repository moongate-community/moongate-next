using Moongate.UO.Data.Data.Tiles;

namespace Moongate.Server.Interfaces.Services.Movement;

/// <summary>
///     Reads map/tile data required by movement validation.
/// </summary>
public interface IMovementTileQueryService
{
    /// <summary>Returns the map's tile dimensions, or false when the map is unknown.</summary>
    bool TryGetMapBounds(int mapId, out int width, out int height);

    /// <summary>Returns the land tile at world coordinates, or false when the map is unknown.</summary>
    bool TryGetLandTile(int mapId, int x, int y, out LandTile landTile);

    /// <summary>Returns the static tiles at world coordinates (empty when the map is unknown).</summary>
    IReadOnlyList<StaticTile> GetStaticTiles(int mapId, int x, int y);
}
