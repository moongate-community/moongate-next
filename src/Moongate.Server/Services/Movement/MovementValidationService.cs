using Moongate.Core.Geometry;
using Moongate.Core.Types;
using Moongate.Server.Interfaces.Services.Movement;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Server.Services.Movement;

/// <summary>
///     Validates player movement against map land/static tiles (bounds, diagonal, Z step, blocking)
///     and mobile-vs-mobile collision via the spatial index.
/// </summary>
public sealed class MovementValidationService : IMovementValidationService
{
    private const int PersonHeight = 16;
    private const int StepHeight = 2;
    private const int FallbackStepHeight = 16;
    private readonly IWorldSpatialIndex _index;
    private readonly ITileDataStore _tileData;

    private readonly IMovementTileQueryService _tiles;

    public MovementValidationService(IMovementTileQueryService tiles, ITileDataStore tileData, IWorldSpatialIndex index)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(tileData);
        ArgumentNullException.ThrowIfNull(index);

        _tiles = tiles;
        _tileData = tileData;
        _index = index;
    }

    public bool TryResolveMove(MobileEntity mobile, DirectionType direction, out Point3D newLocation)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        var currentLocation = mobile.Location;
        var destination = currentLocation.Move(direction);
        newLocation = currentLocation;

        if (!_tiles.TryGetMapBounds(mobile.MapId, out var width, out var height))
        {
            newLocation = destination;

            return true;
        }

        if (!IsInsideMap(width, height, destination.X, destination.Y))
        {
            return false;
        }

        var baseDirection = Point3D.GetBaseDirection(direction);

        if (IsDiagonal(baseDirection) && !CanMoveDiagonal(mobile, currentLocation, destination))
        {
            return false;
        }

        if (!TryResolveDestinationZ(mobile, currentLocation, destination, out var resolvedZ))
        {
            return false;
        }

        if (IsBlockedByStatics(mobile.MapId, destination, resolvedZ))
        {
            return false;
        }

        if (IsBlockedByMobiles(mobile, destination, resolvedZ))
        {
            return false;
        }

        newLocation = new Point3D(destination.X, destination.Y, resolvedZ);

        return true;
    }

    private static bool IsInsideMap(int width, int height, int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    private static bool IsDiagonal(DirectionType direction)
    {
        return direction is DirectionType.NorthEast or DirectionType.SouthEast
            or DirectionType.SouthWest or DirectionType.NorthWest;
    }

    private bool CanMoveDiagonal(MobileEntity mobile, Point3D current, Point3D destination)
    {
        var sideA = new Point3D(destination.X, current.Y, current.Z);
        var sideB = new Point3D(current.X, destination.Y, current.Z);

        return IsTileWalkable(mobile.MapId, current.Z, sideA) && IsTileWalkable(mobile.MapId, current.Z, sideB);
    }

    private bool IsTileWalkable(int mapId, int startZ, Point3D location)
    {
        var supports = CollectSupports(mapId, location.X, location.Y);

        return supports.Count != 0 && SelectBestSupport(supports, startZ, FallbackStepHeight).HasValue;
    }

    private List<int> CollectSupports(int mapId, int x, int y)
    {
        var supports = new List<int>(8);

        if (_tiles.TryGetLandTile(mapId, x, y, out var landTile))
        {
            var landFlags = _tileData.GetLand(landTile.ID).Flags;

            if (!landTile.Ignored && (landFlags & UoTileFlag.Impassable) == 0)
            {
                supports.Add(landTile.Z);
            }
        }

        foreach (var staticTile in _tiles.GetStaticTiles(mapId, x, y))
        {
            var itemData = _tileData.GetItem(staticTile.ID);
            var isStair = itemData[UoTileFlag.StairBack] || itemData[UoTileFlag.StairRight];

            if (!itemData.Surface && !itemData.Bridge && !isStair)
            {
                continue;
            }

            supports.Add(staticTile.Z + itemData.CalcHeight);
        }

        return supports;
    }

    private bool IsBlockedByStatics(int mapId, Point3D destination, int z)
    {
        var ourTop = z + PersonHeight;

        foreach (var staticTile in _tiles.GetStaticTiles(mapId, destination.X, destination.Y))
        {
            var itemData = _tileData.GetItem(staticTile.ID);
            var isStair = itemData[UoTileFlag.StairBack] || itemData[UoTileFlag.StairRight];
            var isWalkableSupport =
                (itemData.Surface || itemData.Bridge || isStair) && !itemData.Impassable && !itemData.Wall;

            if (isWalkableSupport)
            {
                continue;
            }

            if (!itemData.Impassable && !itemData.ImpassableSurface && !itemData.Wall)
            {
                continue;
            }

            var checkZ = staticTile.Z;
            var checkTop = checkZ + Math.Max(1, itemData.CalcHeight);

            if (checkTop > z && ourTop > checkZ)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockedByMobiles(MobileEntity mobile, Point3D destination, int resolvedZ)
    {
        var ourTop = resolvedZ + PersonHeight;

        foreach (var other in _index.GetMobilesInRange(mobile.MapId, destination, 1))
        {
            if (other.Id == mobile.Id)
            {
                continue;
            }

            if (other.Location.X != destination.X || other.Location.Y != destination.Y)
            {
                continue;
            }

            var otherTop = other.Location.Z + PersonHeight;

            if (otherTop > resolvedZ && ourTop > other.Location.Z)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryResolveDestinationZ(MobileEntity mobile, Point3D current, Point3D destination, out int resolvedZ)
    {
        resolvedZ = current.Z;
        var startZ = current.Z;
        var supports = CollectSupports(mobile.MapId, destination.X, destination.Y);

        if (supports.Count == 0)
        {
            return false;
        }

        var strict = SelectBestSupport(supports, startZ, StepHeight);
        var fallback = SelectBestSupport(supports, startZ, FallbackStepHeight);
        var upward = SelectUpwardSupport(supports, startZ, FallbackStepHeight);

        if (upward.HasValue)
        {
            resolvedZ = upward.Value;

            return true;
        }

        if (strict.HasValue && fallback.HasValue && fallback.Value > strict.Value)
        {
            resolvedZ = fallback.Value;

            return true;
        }

        if (strict.HasValue)
        {
            resolvedZ = strict.Value;

            return true;
        }

        if (fallback.HasValue)
        {
            resolvedZ = fallback.Value;

            return true;
        }

        return false;
    }

    private static int? SelectBestSupport(IReadOnlyList<int> supports, int startZ, int stepHeight)
    {
        int? best = null;
        var bestDiff = int.MaxValue;

        for (var i = 0; i < supports.Count; i++)
        {
            var candidate = supports[i];

            if (candidate > startZ + stepHeight || candidate < startZ - PersonHeight)
            {
                continue;
            }

            var diff = Math.Abs(candidate - startZ);

            if (diff < bestDiff || diff == bestDiff && (!best.HasValue || candidate > best.Value))
            {
                best = candidate;
                bestDiff = diff;
            }
        }

        return best;
    }

    private static int? SelectUpwardSupport(IReadOnlyList<int> supports, int startZ, int stepHeight)
    {
        int? best = null;

        for (var i = 0; i < supports.Count; i++)
        {
            var candidate = supports[i];

            if (candidate <= startZ || candidate > startZ + stepHeight)
            {
                continue;
            }

            if (!best.HasValue || candidate < best.Value)
            {
                best = candidate;
            }
        }

        return best;
    }
}
