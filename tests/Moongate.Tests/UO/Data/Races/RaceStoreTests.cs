using Moongate.UO.Data.Races;

namespace Moongate.Tests.UO.Data.Races;

public class RaceStoreTests
{
    private const string Toml =
        """
        [[race]]
        id = 0
        index = 0
        name = "Human"
        plural_name = "Humans"
        male_body = 400
        female_body = 401
        male_ghost_body = 402
        female_ghost_body = 403

        [[race]]
        id = 2
        index = 2
        name = "Gargoyle"
        plural_name = "Gargoyles"
        male_body = 666
        female_body = 667
        male_ghost_body = 970
        female_ghost_body = 971
        """;

    [Fact]
    public void Load_ParsesRaces_AndComputesFlag()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "races.toml"), Toml);
            var store = new RaceStore(dir.FullName);

            Assert.Equal(2, store.Races.Count);

            var human = store.GetById(0);
            Assert.Equal("Human", human!.Name);
            Assert.Equal(400, human.MaleBody);
            Assert.Equal(1, human.RaceFlag); // 1 << 0

            Assert.Equal(4, store.GetById(2)!.RaceFlag); // 1 << 2
            Assert.Null(store.GetById(5));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyStore()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new RaceStore(dir.FullName);

            Assert.Empty(store.Races);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
