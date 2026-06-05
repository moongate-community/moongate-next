using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Textures;

namespace Moongate.Tests.UO.Data.Textures;

public class TextureStoreTests
{
    [Fact]
    public void GetTexture_DecodesRaw64Square()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            TextureFixture.Write64(dir.FullName, 3, 0x7FFF); // white
            var store = new TextureStore(new UoFileResolver(dir.FullName));

            using var image = store.GetTexture(3);

            Assert.NotNull(image);
            Assert.Equal(64, image!.Width);
            Assert.Equal(64, image.Height);
            var pixel = image[0, 0];
            Assert.Equal((255, 255, 255, 255), (pixel.R, pixel.G, pixel.B, pixel.A));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetTexture_MissingFiles_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new TextureStore(new UoFileResolver(dir.FullName));

            Assert.Null(store.GetTexture(0));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
