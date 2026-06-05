using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Tiles;

namespace Moongate.Tests.UO.Data.Tiles;

public class TileMatrixTests
{
    [Fact]
    public void GetLandTile_ReturnsWrittenCell()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            MapFixture.Write(
                dir.FullName, fileIndex: 0, width: 8, height: 8,
                landCells: [new MapFixture.LandCell(3, 2, 0x0A, 5)],
                statics: []
            );
            using var matrix = new TileMatrix(new UoFileResolver(dir.FullName), 0, 0, 8, 8);

            var tile = matrix.GetLandTile(3, 2);

            Assert.Equal(0x0A, tile.ID);
            Assert.Equal(5, tile.Z);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetStaticTiles_ReturnsWrittenStatic()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            MapFixture.Write(
                dir.FullName, fileIndex: 0, width: 8, height: 8,
                landCells: [],
                statics: [new MapFixture.StaticTileSpec(0, 0, 0x4000, BlockX: 3, BlockY: 2, Z: 10, Hue: 0)]
            );
            using var matrix = new TileMatrix(new UoFileResolver(dir.FullName), 0, 0, 8, 8);

            var tiles = matrix.GetStaticTiles(3, 2);

            Assert.Single(tiles);
            Assert.Equal(0x4000, tiles[0].ID);
            Assert.Equal(10, tiles[0].Z);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetLandTile_MissingFiles_ReturnsZeroTile()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            using var matrix = new TileMatrix(new UoFileResolver(dir.FullName), 0, 0, 8, 8);

            var tile = matrix.GetLandTile(3, 2);

            Assert.Equal(0, tile.ID);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
