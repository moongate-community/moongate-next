using Moongate.UO.Data.Files;
using Moongate.UO.Data.Textures;

namespace Moongate.Tests.UO.Data.Integration;

public class UoImageAssetsIntegrationTests
{
    private static string? ResolveClientDir(string requiredFile)
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        var has = Directory.Exists(candidate) &&
                  Directory.EnumerateFiles(candidate)
                           .Any(f => string.Equals(Path.GetFileName(f), requiredFile, StringComparison.OrdinalIgnoreCase));

        return has ? candidate : null;
    }

    [SkippableFact]
    public void RealTextures_DecodeAtLeastOne()
    {
        var dir = ResolveClientDir("texmaps.mul");
        Skip.If(dir is null, "No texmaps.mul (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var store = new TextureStore(new UoFileResolver(dir!));
        var found = false;

        for (var id = 0; id < 0x1000 && !found; id++)
        {
            using var image = store.GetTexture(id);

            if (image is { Width: > 0, Height: > 0 })
            {
                found = true;
            }
        }

        Assert.True(found);
    }
}
