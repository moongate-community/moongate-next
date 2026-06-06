using Moongate.UO.Data.Bodies;
using Moongate.UO.Data.Types.Bodies;

namespace Moongate.Tests.UO.Data.Bodies;

public class BodyDataStoreTests
{
    private const string Yaml =
        """
        bodies:
          monster: [1, 2, 7]
          animal: [5, 6]
          sea: [150]
          human: [400, 401]
          equipment: []
        """;

    [Fact]
    public void Load_MapsBodyIdsToTypes()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "bodies.yaml"), Yaml);
            var store = new BodyDataStore(dir.FullName);

            Assert.Equal(UoBodyType.Monster, store.GetBodyType(1));
            Assert.Equal(UoBodyType.Animal, store.GetBodyType(5));
            Assert.Equal(UoBodyType.Sea, store.GetBodyType(150));
            Assert.Equal(UoBodyType.Human, store.GetBodyType(400));
            Assert.Equal(UoBodyType.Empty, store.GetBodyType(9999));
            Assert.Equal(UoBodyType.Empty, store.GetBodyType(-1));
            Assert.Equal(8, store.Count);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyTable()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new BodyDataStore(dir.FullName);

            Assert.Equal(0, store.Count);
            Assert.Equal(UoBodyType.Empty, store.GetBodyType(1));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
