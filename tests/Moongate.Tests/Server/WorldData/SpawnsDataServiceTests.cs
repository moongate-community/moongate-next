using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Services.World;
using Moongate.Server.Types.World;

namespace Moongate.Tests.Server.WorldData;

public class SpawnsDataServiceTests
{
    [Fact]
    public void SetEntries_GroupsEntriesByMap()
    {
        var spawnEntries = new List<SpawnEntryDefinition>
        {
            new("Rat", 1, 100)
        };
        var feluccaSpawn = CreateSpawnDefinition("Felucca", 0, spawnEntries);
        var trammelSpawn = CreateSpawnDefinition("Trammel", 1);
        var service = new SpawnsDataService();

        service.SetEntries([feluccaSpawn, trammelSpawn]);
        spawnEntries.Add(new SpawnEntryDefinition("Cat", 1, 100));

        var feluccaEntries = service.GetEntriesByMap(0);
        var trammelEntries = service.GetEntriesByMap(1);
        var missingEntries = service.GetEntriesByMap(2);

        Assert.Collection(
            feluccaEntries,
            entry =>
            {
                Assert.Equal("Felucca", entry.Map);
                Assert.Single(entry.Entries);
            }
        );
        Assert.Collection(
            trammelEntries,
            entry => Assert.Equal("Trammel", entry.Map)
        );
        Assert.Empty(missingEntries);
    }

    private static SpawnDefinitionEntry CreateSpawnDefinition(
        string map,
        int mapId,
        IReadOnlyList<SpawnEntryDefinition>? entries = null
    )
    {
        return new SpawnDefinitionEntry(
            mapId,
            map,
            "shared",
            $"{map}.yaml",
            Guid.NewGuid(),
            SpawnDefinitionKind.Spawner,
            $"{map} Spawner",
            new Point3D(100, 200, 0),
            1,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(2),
            0,
            10,
            10,
            entries ?? [new SpawnEntryDefinition("Rat", 1, 100)]
        );
    }
}
