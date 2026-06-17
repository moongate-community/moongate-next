using Moongate.Server.Interfaces.Services.Movement;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Maps;

namespace Moongate.Server.Services.Movement;

/// <summary>
/// Reads map bounds and tiles from <see cref="IMapService" /> for movement validation.
/// </summary>
public sealed class MovementTileQueryService : IMovementTileQueryService
{
    private readonly IMapService _maps;

    public MovementTileQueryService(IMapService maps)
    {
        ArgumentNullException.ThrowIfNull(maps);

        _maps = maps;
    }

    public bool TryGetMapBounds(int mapId, out int width, out int height)
    {
        var map = _maps.GetMap(mapId);

        if (map is null)
        {
            width = 0;
            height = 0;

            return false;
        }

        width = map.Width;
        height = map.Height;

        return true;
    }

    public bool TryGetLandTile(int mapId, int x, int y, out LandTile landTile)
    {
        var map = _maps.GetMap(mapId);

        if (map is null)
        {
            landTile = default;

            return false;
        }

        landTile = map.GetLandTile(x, y);

        return true;
    }

    public IReadOnlyList<StaticTile> GetStaticTiles(int mapId, int x, int y)
        => _maps.GetMap(mapId)?.GetStaticTiles(x, y) ?? [];
}
