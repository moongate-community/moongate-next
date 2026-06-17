using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;
using Moongate.Server.Types.World;

namespace Moongate.Tests.Server.WorldData;

public sealed class SpawnsDataLoaderTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-spawns-loader-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }

    [Fact]
    public void LoadSpawns_MissingSpawnsDirectory_ClearsEntries()
    {
        var service = new SpawnsDataService();
        service.SetEntries(
            [
                new SpawnDefinitionEntry(
                    0,
                    "felucca",
                    "shared/felucca",
                    "Vendors.yaml",
                    Guid.NewGuid(),
                    SpawnDefinitionKind.Spawner,
                    "Existing Spawner",
                    new Point3D(100, 200, 0),
                    1,
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    0,
                    10,
                    10,
                    [new SpawnEntryDefinition("Vendor", 1, 100)]
                )
            ]
        );
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadSpawns(service);

        Assert.Empty(service.GetAllEntries());
        Assert.Empty(service.GetEntriesByMap(0));
    }

    [Fact]
    public void LoadSpawns_WithBlankMap_InfersMapFromSourceGroup()
    {
        WriteSpawnYaml("shared/tokuno/Vendors.yaml", "");
        var service = new SpawnsDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadSpawns(service);

        var entry = Assert.Single(service.GetAllEntries());
        Assert.Equal(4, entry.MapId);
        Assert.Equal("tokuno", entry.Map);
        Assert.Equal("shared/tokuno", entry.SourceGroup);
    }

    [Fact]
    public void LoadSpawns_WithBlankMapInNestedPath_InfersMapFromKnownSourceGroupSegment()
    {
        WriteSpawnYaml("shared/felucca/towns/Vendors.yaml", "");
        var service = new SpawnsDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadSpawns(service);

        var entry = Assert.Single(service.GetAllEntries());
        Assert.Equal(0, entry.MapId);
        Assert.Equal("felucca", entry.Map);
        Assert.Equal("shared/felucca/towns", entry.SourceGroup);
    }

    [Fact]
    public void LoadSpawns_WithSiblingAndNestedFiles_LoadsInNormalizedSourcePathOrder()
    {
        WriteSpawnYaml("shared/trammel/towns/Vendors.yaml", "Trammel", "Town Vendor Spawner");
        WriteSpawnYaml("shared/trammel/Animals.yaml", "Trammel", "Animal Spawner");
        var service = new SpawnsDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadSpawns(service);

        var entries = service.GetAllEntries();
        Assert.Collection(
            entries,
            entry =>
            {
                Assert.Equal("Animal Spawner", entry.Name);
                Assert.Equal("shared/trammel", entry.SourceGroup);
                Assert.Equal("Animals.yaml", entry.SourceFile);
            },
            entry =>
            {
                Assert.Equal("Town Vendor Spawner", entry.Name);
                Assert.Equal("shared/trammel/towns", entry.SourceGroup);
                Assert.Equal("Vendors.yaml", entry.SourceFile);
            }
        );
    }

    [Fact]
    public void LoadSpawns_WithSpawnYaml_LoadsEntriesWithSourceMetadata()
    {
        WriteSpawnYaml("shared/felucca/Vendors.yaml", "Felucca");
        var service = new SpawnsDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadSpawns(service);

        var entry = Assert.Single(service.GetAllEntries());
        Assert.Equal(0, entry.MapId);
        Assert.Equal("felucca", entry.Map);
        Assert.Equal("shared/felucca", entry.SourceGroup);
        Assert.Equal("Vendors.yaml", entry.SourceFile);
        Assert.Single(entry.Entries);
    }

    private void WriteSpawnYaml(string relativePath, string map, string name = "Vendor Spawner")
    {
        var path = Path.Combine(_dataDirectory, "spawns", relativePath);
        var directory = Path.GetDirectoryName(path);

        Assert.False(string.IsNullOrWhiteSpace(directory));
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            path,
            $"""
             spawn:
               - type: Spawner
                 guid: 001a5320-820c-4300-96f9-676e428b55be
                 name: {name}
                 location: [4066, 569, 0]
                 map: "{map}"
                 count: 1
                 min_delay: 00:05:00
                 max_delay: 00:10:00
                 team: 0
                 home_range: 80
                 walking_range: 80
                 entries:
                   - name: Baker
                     max_count: 1
                     probability: 100
             """
        );
    }
}
