using System.Collections.Generic;
using Moongate.Core.Geometry;
using Moongate.Core.Types;
using Moongate.Server.Interfaces.Services.Movement;
using Moongate.Server.Services.Movement;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;
using Xunit;

namespace Moongate.Tests.Server.Movement;

public sealed class MovementValidationServiceTests
{
    [Fact]
    public void TryResolveMove_FlatWalkableLand_MovesToTileAtLandZ()
    {
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0);
        var service = new MovementValidationService(tiles, new FakeTileData());
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(50, 50, 0) };

        Assert.True(service.TryResolveMove(mobile, DirectionType.South, out var dest));
        Assert.Equal(new Point3D(50, 51, 0), dest);
    }

    [Fact]
    public void TryResolveMove_OffMap_ReturnsFalse()
    {
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0);
        var service = new MovementValidationService(tiles, new FakeTileData());
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(0, 0, 0) };

        Assert.False(service.TryResolveMove(mobile, DirectionType.West, out _)); // x -> -1
    }

    [Fact]
    public void TryResolveMove_ImpassableStatic_Blocks()
    {
        // a wall static at the destination, height 20, overlaps the person box
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0)
            .WithStatic(50, 51, new StaticTile((ushort)100, (sbyte)0));
        var data = new FakeTileData().WithItem(
            100,
            new ItemData { Flags = UoTileFlag.Impassable | UoTileFlag.Wall, Height = 20 }
        );
        var service = new MovementValidationService(tiles, data);
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(50, 50, 0) };

        Assert.False(service.TryResolveMove(mobile, DirectionType.South, out _));
    }

    [Fact]
    public void TryResolveMove_UnknownMap_PassesThroughToDestination()
    {
        // No bounds known for the map -> permissive pass-through at the raw moved point (unchanged Z).
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0, mapKnown: false);
        var service = new MovementValidationService(tiles, new FakeTileData());
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(50, 50, 7) };

        Assert.True(service.TryResolveMove(mobile, DirectionType.South, out var dest));
        Assert.Equal(new Point3D(50, 51, 7), dest); // South: (50,50)->(50,51), Z unchanged
    }

    [Fact]
    public void TryResolveMove_DiagonalWithBlockedSide_Denies()
    {
        // SouthEast from (50,50) -> (51,51); orthogonal sides are (51,50) and (50,51).
        // Make side (51,50) unwalkable: Ignored land id (2) and no statics -> zero supports.
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0)
            .WithLand(51, 50, new LandTile((short)2, (sbyte)0));
        var service = new MovementValidationService(tiles, new FakeTileData());
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(50, 50, 0) };

        Assert.False(service.TryResolveMove(mobile, DirectionType.SouthEast, out _));
    }

    [Fact]
    public void TryResolveMove_SurfaceStaticAbove_StepsUp()
    {
        // Destination (50,51) has flat land support (z=0) plus a walkable Surface static at z=2.
        // CalcHeight for a non-bridge == Height (0), so the static support is 2+0 = 2.
        // SelectUpwardSupport picks the lowest candidate > startZ(0) and <= startZ+16 -> 2.
        var tiles = new FakeTileQuery(width: 100, height: 100, landZ: 0)
            .WithStatic(50, 51, new StaticTile((ushort)200, (sbyte)2));
        var data = new FakeTileData().WithItem(
            200,
            new ItemData { Flags = UoTileFlag.Surface, Height = 0 }
        );
        var service = new MovementValidationService(tiles, data);
        var mobile = new MobileEntity { MapId = 0, Location = new Point3D(50, 50, 0) };

        Assert.True(service.TryResolveMove(mobile, DirectionType.South, out var dest));
        Assert.Equal(new Point3D(50, 51, 2), dest);
    }

    private sealed class FakeTileQuery : IMovementTileQueryService
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _landZ;
        private readonly bool _mapKnown;
        private readonly Dictionary<(int, int), List<StaticTile>> _statics;
        private readonly Dictionary<(int, int), LandTile> _landOverrides;

        public FakeTileQuery(int width, int height, int landZ, bool mapKnown = true)
        {
            _width = width;
            _height = height;
            _landZ = landZ;
            _mapKnown = mapKnown;
            _statics = new Dictionary<(int, int), List<StaticTile>>();
            _landOverrides = new Dictionary<(int, int), LandTile>();
        }

        public FakeTileQuery WithStatic(int x, int y, StaticTile s)
        {
            if (!_statics.TryGetValue((x, y), out var list))
            {
                list = new List<StaticTile>();
                _statics[(x, y)] = list;
            }

            list.Add(s);

            return this;
        }

        public FakeTileQuery WithLand(int x, int y, LandTile landTile)
        {
            _landOverrides[(x, y)] = landTile;

            return this;
        }

        public bool TryGetMapBounds(int mapId, out int width, out int height)
        {
            width = _width;
            height = _height;

            return _mapKnown;
        }

        public bool TryGetLandTile(int mapId, int x, int y, out LandTile landTile)
        {
            landTile = _landOverrides.TryGetValue((x, y), out var ovr)
                ? ovr
                : new LandTile((short)1, (sbyte)_landZ);

            return true;
        }

        public IReadOnlyList<StaticTile> GetStaticTiles(int mapId, int x, int y)
            => _statics.TryGetValue((x, y), out var list) ? list : new List<StaticTile>();
    }

    private sealed class FakeTileData : ITileDataStore
    {
        private readonly Dictionary<int, ItemData> _items;

        public FakeTileData()
        {
            _items = new Dictionary<int, ItemData>();
        }

        public IReadOnlyList<LandData> LandTable => System.Array.Empty<LandData>();

        public IReadOnlyList<ItemData> ItemTable => System.Array.Empty<ItemData>();

        public FakeTileData WithItem(int id, ItemData d)
        {
            _items[id] = d;

            return this;
        }

        public ItemData GetItem(int id)
            => _items.TryGetValue(id, out var item) ? item : default;

        public LandData GetLand(int id)
            => default;
    }
}
