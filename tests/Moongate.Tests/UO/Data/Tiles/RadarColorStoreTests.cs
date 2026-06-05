using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Tiles;

namespace Moongate.Tests.UO.Data.Tiles;

public class RadarColorStoreTests
{
    [Fact]
    public void GetLandAndStaticColor_ReturnExpandedRgb()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            RadarColFixture.Write(dir.FullName, new Dictionary<int, ushort>
            {
                [5] = 0x7FFF,           // land tile 5 -> white
                [0x4000 + 10] = 0x001F  // static tile 10 -> blue
            });
            var store = new RadarColorStore(new UoFileResolver(dir.FullName));

            Assert.Equal(((byte)255, (byte)255, (byte)255), store.GetLandColor(5));
            Assert.Equal(((byte)0, (byte)0, (byte)255), store.GetStaticColor(10));
            Assert.Equal(0x8000, store.Count);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsZeroedTable()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new RadarColorStore(new UoFileResolver(dir.FullName));

            Assert.Equal(((byte)0, (byte)0, (byte)0), store.GetLandColor(5));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
