using Moongate.Server.Bootstrap;
using Serilog;

namespace Moongate.Tests.Hosting;

public class UoDataAssetsBootstrapperTests
{
    [Fact]
    public void EnsureDataAssets_CopiesMissing_SkipsExisting()
    {
        var src = Directory.CreateTempSubdirectory("nr-src-");
        var dst = Directory.CreateTempSubdirectory("nr-dst-");

        try
        {
            File.WriteAllText(Path.Combine(src.FullName, "a.yaml"), "new");
            File.WriteAllText(Path.Combine(src.FullName, "b.yaml"), "src-b");
            var nestedSource = Directory.CreateDirectory(Path.Combine(src.FullName, "locations"));
            File.WriteAllText(Path.Combine(nestedSource.FullName, "trammel.yaml"), "name: Trammel");
            File.WriteAllText(Path.Combine(dst.FullName, "b.yaml"), "existing-b");

            var copied = UoDataAssetsBootstrapper.EnsureDataAssets(src.FullName, dst.FullName, Log.Logger);

            Assert.Equal(2, copied);
            Assert.Equal("new", File.ReadAllText(Path.Combine(dst.FullName, "a.yaml")));
            Assert.Equal("existing-b", File.ReadAllText(Path.Combine(dst.FullName, "b.yaml"))); // not overwritten
            Assert.Equal(
                "name: Trammel",
                File.ReadAllText(Path.Combine(dst.FullName, "locations", "trammel.yaml"))
            );
        }
        finally
        {
            src.Delete(true);
            dst.Delete(true);
        }
    }

    [Fact]
    public void EnsureDataAssets_MissingSource_ReturnsZero()
    {
        var dst = Directory.CreateTempSubdirectory("nr-dst-");

        try
        {
            var missingSource = Path.Combine(dst.FullName, "does-not-exist");

            var copied = UoDataAssetsBootstrapper.EnsureDataAssets(missingSource, dst.FullName, Log.Logger);

            Assert.Equal(0, copied);
        }
        finally
        {
            dst.Delete(true);
        }
    }
}
