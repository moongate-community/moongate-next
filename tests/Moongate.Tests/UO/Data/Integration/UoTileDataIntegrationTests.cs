using Moongate.UO.Data.Files;
using Moongate.UO.Data.Tiles;

namespace Moongate.Tests.UO.Data.Integration;

public class UoTileDataIntegrationTests
{
    [SkippableFact]
    public void Load_RealTileData_PopulatesTables()
    {
        var dir = ResolveClientDir();
        Skip.If(dir is null, "No UO client files (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var store = new TileDataStore(new UoFileResolver(dir!));

        Assert.Equal(0x4000, store.LandTable.Count);
        Assert.True(store.ItemTable.Count >= 0x4000);
        Assert.Contains(store.ItemTable, item => !string.IsNullOrEmpty(item.Name));
    }

    private static string? ResolveClientDir()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        return Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "tiledata.mul"))
            ? candidate
            : null;
    }
}
