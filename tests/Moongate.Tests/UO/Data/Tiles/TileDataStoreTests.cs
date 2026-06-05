using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.UO.Data.Tiles;

public class TileDataStoreTests
{
    [Fact]
    public void Constructor_MissingTileData_Throws()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            Assert.Throws<FileNotFoundException>(() => new TileDataStore(new UoFileResolver(dir.FullName)));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Load_ParsesLandAndItemEntries()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            TileDataFixture.Write(
                dir.FullName,
                [new(3, (uint)UoTileFlag.Impassable, "grass")],
                [new(0x0A, (uint)UoTileFlag.Weapon, "dagger", 1, 0, 0, 0, 0, 5)]
            );
            var store = new TileDataStore(new UoFileResolver(dir.FullName));

            var land = store.GetLand(3);
            var item = store.GetItem(0x0A);

            Assert.Equal("grass", land.Name);
            Assert.Equal(UoTileFlag.Impassable, land.Flags);
            Assert.Equal("dagger", item.Name);
            Assert.True(item.Weapon);
            Assert.Equal(1, item.Weight);
            Assert.Equal(5, item.Height);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
