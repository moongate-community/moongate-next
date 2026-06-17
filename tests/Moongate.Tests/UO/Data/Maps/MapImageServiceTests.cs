using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Data.Maps;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types.Maps;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Maps;

public class MapImageServiceTests
{
    [Fact]
    public void GetMapImage_MissingMap_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("moongate-map-image-");

        try
        {
            var service = BuildService(dir.FullName);

            using var image = service.GetMapImage(99);

            Assert.Null(image);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetMapImage_MissingMapFiles_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("moongate-map-image-");

        try
        {
            RadarColFixture.Write(dir.FullName, new Dictionary<int, ushort>());
            var service = BuildService(dir.FullName);

            using var image = service.GetMapImage(0);

            Assert.Null(image);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetMapImage_RenderedMap_UsesLandRadarColor()
    {
        var dir = Directory.CreateTempSubdirectory("moongate-map-image-");

        try
        {
            MapFixture.Write(
                dir.FullName,
                0,
                8,
                8,
                [new MapFixture.LandCell(3, 2, 5, 0)],
                []
            );
            RadarColFixture.Write(dir.FullName, new Dictionary<int, ushort> { [5] = 0x7FFF });
            var service = BuildService(dir.FullName);

            using var image = service.GetMapImage(0);

            Assert.NotNull(image);
            Assert.Equal(8, image!.Width);
            Assert.Equal(8, image.Height);
            using var pixels = image.CloneAs<Rgb24>();
            Assert.Equal(new Rgb24(255, 255, 255), pixels[3, 2]);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetMapImage_StaticAboveLand_UsesStaticRadarColor()
    {
        var dir = Directory.CreateTempSubdirectory("moongate-map-image-");

        try
        {
            MapFixture.Write(
                dir.FullName,
                0,
                8,
                8,
                [new MapFixture.LandCell(3, 2, 5, 0)],
                [new MapFixture.StaticTileSpec(0, 0, 10, 3, 2, 0, 0)]
            );
            RadarColFixture.Write(
                dir.FullName,
                new Dictionary<int, ushort>
                {
                    [5] = 0x7FFF,
                    [0x4000 + 10] = 0x001F
                }
            );
            var service = BuildService(dir.FullName, new ItemData("wall", default, 0, 0, 0, 0, 0, 1));

            using var image = service.GetMapImage(0);

            Assert.NotNull(image);
            using var pixels = image!.CloneAs<Rgb24>();
            Assert.Equal(new Rgb24(0, 0, 255), pixels[3, 2]);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    private static MapImageService BuildService(string directory, ItemData staticTileData = default)
    {
        var resolver = new UoFileResolver(directory);
        var map = new Map(new MapDefinition(0, 0, 0, 8, 8, "Test", MapRulesType.FeluccaRules, SeasonType.Spring), resolver);

        return new MapImageService(
            new TestMapService(map),
            resolver,
            new RadarColorStore(resolver),
            new TestTileDataStore(staticTileData)
        );
    }

    private sealed class TestMapService : IMapService
    {
        private readonly Map _map;

        public TestMapService(Map map)
        {
            _map = map;
        }

        public IReadOnlyList<Map> Maps => [_map];

        public Map? GetMap(int mapId)
        {
            return mapId == _map.MapId ? _map : null;
        }
    }

    private sealed class TestTileDataStore : ITileDataStore
    {
        private readonly ItemData _staticTileData;

        public TestTileDataStore(ItemData staticTileData)
        {
            _staticTileData = staticTileData;
        }

        public IReadOnlyList<LandData> LandTable => [];

        public IReadOnlyList<ItemData> ItemTable => [];

        public ItemData GetItem(int id)
        {
            return _staticTileData;
        }

        public LandData GetLand(int id)
        {
            return default;
        }
    }
}
