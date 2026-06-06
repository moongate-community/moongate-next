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
            File.WriteAllText(Path.Combine(dst.FullName, "b.yaml"), "existing-b");

            var copied = UoDataAssetsBootstrapper.EnsureDataAssets(src.FullName, dst.FullName, Log.Logger);

            Assert.Equal(1, copied);
            Assert.Equal("new", File.ReadAllText(Path.Combine(dst.FullName, "a.yaml")));
            Assert.Equal("existing-b", File.ReadAllText(Path.Combine(dst.FullName, "b.yaml"))); // not overwritten
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
