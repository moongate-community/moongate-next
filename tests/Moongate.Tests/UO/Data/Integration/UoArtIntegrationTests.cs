using Moongate.UO.Data.Art;
using Moongate.UO.Data.Files;

namespace Moongate.Tests.UO.Data.Integration;

public class UoArtIntegrationTests
{
    [SkippableFact]
    public void GetArt_RealFiles_DecodesAtLeastOneItem()
    {
        var dir = ResolveClientDir();
        Skip.If(dir is null, "No UO art files (set NR_UO_CLIENT_DIR or place files in ~/uo).");

        var service = new ArtService(new UoFileResolver(dir!));

        var found = false;

        for (var id = 0; id < 0x4000 && !found; id++)
        {
            using var image = service.GetArt(id);

            if (image is { Width: > 0, Height: > 0 })
            {
                found = true;
            }
        }

        Assert.True(found);
    }

    private static string? ResolveClientDir()
    {
        var fromEnv = Environment.GetEnvironmentVariable("NR_UO_CLIENT_DIR");
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidate = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : Path.Combine(home, "uo");

        var hasArt = Directory.Exists(candidate) &&
                     Directory.EnumerateFiles(candidate)
                              .Any(
                                  f =>
                                  {
                                      var name = Path.GetFileName(f);

                                      return string.Equals(name, "artLegacyMUL.uop", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(name, "art.mul", StringComparison.OrdinalIgnoreCase);
                                  }
                              );

        return hasArt ? candidate : null;
    }
}
