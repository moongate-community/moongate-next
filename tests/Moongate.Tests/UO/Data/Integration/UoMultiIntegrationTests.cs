using Moongate.UO.Data.Files;
using Moongate.UO.Data.Multi;

namespace Moongate.Tests.UO.Data.Integration;

public class UoMultiIntegrationTests
{
    private static string? ResolveClientDir()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        var hasMulti = Directory.Exists(candidate) &&
                       Directory.EnumerateFiles(candidate).Any(f =>
                       {
                           var name = Path.GetFileName(f);
                           return string.Equals(name, "MultiCollection.uop", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(name, "multi.mul", StringComparison.OrdinalIgnoreCase);
                       });

        return hasMulti ? candidate : null;
    }

    [SkippableFact]
    public void Load_RealMultis_HasComponentsWithGeometry()
    {
        var dir = ResolveClientDir();
        Skip.If(dir is null, "No UO multi files (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var store = new MultiDataStore(new UoFileResolver(dir!));

        Assert.True(store.Count > 0);

        var anyWithGeometry = Enumerable.Range(0, 0x4000)
            .Select(store.GetComponents)
            .Any(m => m.Width > 0 && m.List.Length > 0);

        Assert.True(anyWithGeometry);
    }
}
