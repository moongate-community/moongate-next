using Moongate.UO.Data.Files;
using Moongate.UO.Data.Maps;

namespace Moongate.Tests.UO.Data.Integration;

public class UoMapIntegrationTests
{
    private static string? ResolveClientDir()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        var hasMap = Directory.Exists(candidate) &&
                     (File.Exists(Path.Combine(candidate, "map0.mul")) ||
                      File.Exists(Path.Combine(candidate, "map0LegacyMUL.uop")));

        return hasMap ? candidate : null;
    }

    [SkippableFact]
    public void ReadRealFelucca_ReturnsLandTilesInValidRange()
    {
        var dir = ResolveClientDir();
        Skip.If(dir is null, "No UO client map files (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var service = new MapService(new UoFileResolver(dir!));
        var felucca = service.GetMap(0);

        Assert.NotNull(felucca);

        var tile = felucca!.GetLandTile(1500, 1500);

        Assert.InRange(tile.ID, 0, 0x3FFF);
    }
}
