using Moongate.UO.Data.Bodies;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Races;
using Moongate.UO.Data.Skills;

namespace Moongate.Tests.UO.Data;

public class ShippedUoDataTests
{
    [Fact]
    public void ShippedBodies_ParseToManyEntries()
    {
        var store = new BodyDataStore(UoFilesDirectory());

        Assert.True(store.Count > 1000);
    }

    [Fact]
    public void ShippedExpansions_ParseToTwelve()
    {
        var store = new ExpansionStore(UoFilesDirectory());

        Assert.Equal(12, store.Count);
        Assert.Equal("Age of Shadows", store.GetInfo(5)!.Name);
    }

    [Fact]
    public void ShippedRaces_ParseToThree()
    {
        var store = new RaceStore(UoFilesDirectory());

        Assert.Equal(3, store.Races.Count);
        Assert.Equal("Human", store.GetById(0)!.Name);
    }

    [Fact]
    public void ShippedSkills_ParseAndAreComplete()
    {
        var store = new SkillDataStore(UoFilesDirectory());

        Assert.True(store.Count >= 50);
        Assert.NotNull(store.GetByName("Alchemy"));
    }

    private static string UoFilesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Moongate.Server", "Assets", "uo_files");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/Moongate.Server/uo_files from the test output.");
    }
}
