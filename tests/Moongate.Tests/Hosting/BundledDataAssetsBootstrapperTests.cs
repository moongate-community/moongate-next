using Moongate.Server.Bootstrap;
using Serilog;

namespace Moongate.Tests.Hosting;

public class BundledDataAssetsBootstrapperTests
{
    [Fact]
    public void EnsureDataAssets_CopiesEmbeddedYamlAssets_AndSkipsExistingFiles()
    {
        var dst = Directory.CreateTempSubdirectory("mg-data-");

        try
        {
            var customBodiesPath = Path.Combine(dst.FullName, "uo_files", "bodies.yaml");
            Directory.CreateDirectory(Path.GetDirectoryName(customBodiesPath)!);
            File.WriteAllText(customBodiesPath, "custom bodies");

            var copied = BundledDataAssetsBootstrapper.EnsureDataAssets(
                typeof(MoongateBootstrap).Assembly,
                BundledDataAssetsBootstrapper.DataResourcePrefix,
                dst.FullName,
                Log.Logger
            );

            Assert.True(copied > 200);
            Assert.Equal("custom bodies", File.ReadAllText(customBodiesPath));
            Assert.True(File.Exists(Path.Combine(dst.FullName, "bodyTable.yaml")));
            Assert.True(File.Exists(Path.Combine(dst.FullName, "components", "doors.yaml")));
            Assert.True(File.Exists(Path.Combine(dst.FullName, "expansions.yaml")));
            Assert.True(File.Exists(Path.Combine(dst.FullName, "uo_files", "expansions.yaml")));
            Assert.True(File.Exists(Path.Combine(dst.FullName, "uo_files", "skills.yaml")));
        }
        finally
        {
            dst.Delete(true);
        }
    }

    [Fact]
    public void EnsureDataAssets_MissingEmbeddedPrefix_ReturnsZero()
    {
        var dst = Directory.CreateTempSubdirectory("mg-data-");

        try
        {
            var copied = BundledDataAssetsBootstrapper.EnsureDataAssets(
                typeof(MoongateBootstrap).Assembly,
                "Moongate.Server.Assets.missing",
                dst.FullName,
                Log.Logger
            );

            Assert.Equal(0, copied);
            Assert.Empty(Directory.EnumerateFiles(dst.FullName, "*", SearchOption.AllDirectories));
        }
        finally
        {
            dst.Delete(true);
        }
    }
}
