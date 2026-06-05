using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Data.Maps;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.UO.Data.Maps;

public class MapTests
{
    [Fact]
    public void GetLandTile_DelegatesToTileMatrix()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            MapFixture.Write(
                dir.FullName, fileIndex: 0, width: 8, height: 8,
                landCells: [new MapFixture.LandCell(1, 1, 0x15, 3)],
                statics: []
            );
            var definition = new MapDefinition(0, 0, 0, 8, 8, "Test", MapRulesType.FeluccaRules);
            var map = new Map(definition, new UoFileResolver(dir.FullName));

            var tile = map.GetLandTile(1, 1);

            Assert.Equal(0x15, tile.ID);
            Assert.Equal(3, tile.Z);
            Assert.Equal("Test", map.Name);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
