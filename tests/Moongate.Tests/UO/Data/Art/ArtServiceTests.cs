using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Art;
using Moongate.UO.Data.Files;

namespace Moongate.Tests.UO.Data.Art;

public class ArtServiceTests
{
    [Fact]
    public void GetArt_DecodesSingleWhitePixel()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            ArtFixture.WriteWhitePixel(dir.FullName, 0);
            var service = new ArtService(new UoFileResolver(dir.FullName));

            using var image = service.GetArt(0);

            Assert.NotNull(image);
            Assert.Equal(1, image!.Width);
            Assert.Equal(1, image.Height);

            var pixel = image[0, 0];
            Assert.Equal(255, pixel.R);
            Assert.Equal(255, pixel.G);
            Assert.Equal(255, pixel.B);
            Assert.Equal(255, pixel.A);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetArt_NegativeId_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            ArtFixture.WriteWhitePixel(dir.FullName, 0);
            var service = new ArtService(new UoFileResolver(dir.FullName));

            Assert.Null(service.GetArt(-1));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetArt_NoArtFiles_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var service = new ArtService(new UoFileResolver(dir.FullName));

            Assert.Null(service.GetArt(0));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void IsValidArt_ReflectsPresence()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            ArtFixture.WriteWhitePixel(dir.FullName, 0);
            var service = new ArtService(new UoFileResolver(dir.FullName));

            Assert.True(service.IsValidArt(0));
            Assert.False(service.IsValidArt(1));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
