using Moongate.UO.Data.Files;
using Moongate.UO.Data.Hues;
using Moongate.UO.Data.Tiles;

namespace Moongate.Tests.UO.Data.Integration;

public class UoColorDataIntegrationTests
{
    [SkippableFact]
    public void RealHues_LoadManyEntries()
    {
        var dir = ResolveClientDir("hues.mul");
        Skip.If(dir is null, "No hues.mul (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var store = new HueStore(new UoFileResolver(dir!));

        Assert.True(store.Count > 1000);
        Assert.Equal(32, store.GetHue(0)!.Colors.Length);
    }

    [SkippableFact]
    public void RealRadarColors_HaveExpectedSize()
    {
        var dir = ResolveClientDir("radarcol.mul");
        Skip.If(dir is null, "No radarcol.mul (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var store = new RadarColorStore(new UoFileResolver(dir!));

        Assert.Equal(0x8000, store.Count);
    }

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
}
