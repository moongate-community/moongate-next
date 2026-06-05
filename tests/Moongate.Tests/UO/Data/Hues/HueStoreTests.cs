using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Hues;

namespace Moongate.Tests.UO.Data.Hues;

public class HueStoreTests
{
    [Fact]
    public void Load_ParsesGroupOfEightHues()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            HuesFixture.WriteSingleGroup(dir.FullName, firstColor: 0x7FFF, tableStart: 1, tableEnd: 2, name: "TestHue");
            var store = new HueStore(new UoFileResolver(dir.FullName));

            Assert.Equal(8, store.Count);

            var hue = store.GetHue(0);
            Assert.NotNull(hue);
            Assert.Equal(0x7FFF, hue!.Colors[0]);
            Assert.Equal(1, hue.TableStart);
            Assert.Equal("TestHue", hue.Name);
            Assert.Equal(((byte)255, (byte)255, (byte)255), hue.GetRgb(0));
            Assert.Null(store.GetHue(99));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyStore()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new HueStore(new UoFileResolver(dir.FullName));

            Assert.Equal(0, store.Count);
            Assert.Null(store.GetHue(0));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
