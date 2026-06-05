using Moongate.UO.Data.Files;
using Moongate.UO.Data.Localization;

namespace Moongate.Tests.UO.Data.Integration;

public class UoClilocIntegrationTests
{
    private static string? ResolveClientDir()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        var hasCliloc = Directory.Exists(candidate) &&
                        Directory.EnumerateFiles(candidate)
                                 .Any(f => string.Equals(Path.GetFileName(f), "cliloc.enu", StringComparison.OrdinalIgnoreCase));

        return hasCliloc ? candidate : null;
    }

    [SkippableFact]
    public void Load_RealCliloc_HasManyEntries()
    {
        var dir = ResolveClientDir();
        Skip.If(dir is null, "No UO cliloc.enu (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var service = new LocalizationService(new UoFileResolver(dir!));

        Assert.True(service.Count > 1000);
        Assert.False(string.IsNullOrEmpty(service.GetText(1042971)));
    }
}
