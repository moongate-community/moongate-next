using Moongate.UO.Data.Files;

namespace Moongate.Tests.UO.Data.Files;

public class UoFileResolverTests
{
    [Fact]
    public void Resolve_KnownFile_ReturnsAbsolutePath_CaseInsensitive()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var tileData = Path.Combine(dir.FullName, "tiledata.mul");
            File.WriteAllBytes(tileData, [0]);
            var resolver = new UoFileResolver(dir.FullName);

            Assert.Equal(tileData, resolver.Resolve("TileData.MUL"));
            Assert.True(resolver.Contains("tiledata.mul"));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Resolve_UnknownOrUnlistedFile_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            // not part of the known UO file list -> ignored by the scanner
            File.WriteAllBytes(Path.Combine(dir.FullName, "random.txt"), [0]);
            var resolver = new UoFileResolver(dir.FullName);

            Assert.Null(resolver.Resolve("random.txt"));
            Assert.Null(resolver.Resolve("art.mul"));
            Assert.False(resolver.Contains("art.mul"));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
