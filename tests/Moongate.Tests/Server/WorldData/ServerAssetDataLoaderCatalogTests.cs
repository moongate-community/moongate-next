using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Tests.Server.WorldData;

public sealed class ServerAssetDataLoaderCatalogTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-catalog-loader-{Guid.NewGuid():N}");

    [Fact]
    public void LoadCatalogs_WithCatalogYaml_LoadsAllCatalogFamilies()
    {
        WriteCatalogYaml();
        var teleporters = new TeleportersDataService();
        var regions = new RegionDataService();
        var weather = new WeatherDataService();
        var containers = new ContainerDataService();
        var locations = new LocationCatalogService();
        var names = new NameDataService();
        var professions = new ProfessionDataService();
        var signs = new SignDataService();
        var decorations = new DecorationDataService();
        var mounts = new MountDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadCatalogs(
            teleporters,
            regions,
            weather,
            containers,
            locations,
            names,
            professions,
            signs,
            decorations,
            mounts
        );

        var teleporter = Assert.Single(teleporters.GetAllEntries());
        Assert.Equal(0, teleporter.SourceMapId);
        Assert.Equal(1, teleporter.DestinationMapId);
        Assert.True(teleporters.TryGetEntryAtLocation(0, new Point3D(10, 20, 0), out _));

        var region = Assert.Single(regions.GetAllEntries());
        Assert.Equal("TownRegion", region.Type);
        Assert.Equal(0, region.MapId);
        Assert.Equal("britain", region.Name);
        var area = Assert.Single(region.Area);
        Assert.Equal(100, area.X1);
        Assert.Equal(200, area.Y1);
        Assert.Equal(110, area.X2);
        Assert.Equal(220, area.Y2);

        var weatherEntry = Assert.Single(weather.GetAllEntries());
        Assert.Equal(1, weatherEntry.Id);
        Assert.Equal("Desert", weatherEntry.Name);
        Assert.Equal(7, weatherEntry.RainChance);
        Assert.Equal(new WeatherRange(11, 22), weatherEntry.RainIntensity);
        Assert.Equal(3, weatherEntry.RainTemperatureDrop);
        Assert.Equal(4, weatherEntry.SnowChance);
        Assert.Equal(new WeatherRange(5, 6), weatherEntry.SnowIntensity);
        Assert.Equal(2, weatherEntry.SnowThreshold);
        Assert.Equal(8, weatherEntry.StormChance);
        Assert.Equal(new WeatherRange(9, 10), weatherEntry.StormIntensity);
        Assert.Equal(12, weatherEntry.StormTemperatureDrop);
        Assert.Equal(31, weatherEntry.MaxTemperature);
        Assert.Equal(13, weatherEntry.MinTemperature);
        Assert.Equal(14, weatherEntry.ColdChance);
        Assert.Equal(15, weatherEntry.ColdIntensity);
        Assert.Equal(16, weatherEntry.HeatChance);
        Assert.Equal(17, weatherEntry.HeatIntensity);
        Assert.Equal(18, weatherEntry.LightMin);
        Assert.Equal(19, weatherEntry.LightMax);

        var container = Assert.Single(containers.GetAllContainers());
        Assert.Equal("backpack", container.Id);
        Assert.Equal(3701, container.ItemId);
        var layout = Assert.Single(containers.GetAllLayouts());
        Assert.Equal(60, layout.GumpId);
        Assert.Equal([44, 65, 142, 94], layout.Bounds);

        var locationEntries = locations.GetAllLocations();
        Assert.Equal(3, locationEntries.Count);
        Assert.Contains(locationEntries, entry => entry.Name == "Top Level" && entry.CategoryPath == "");
        Assert.Contains(locationEntries, entry => entry.Name == "Bank" && entry.CategoryPath == "Towns");
        Assert.Contains(locationEntries, entry => entry.Name == "Bakery" && entry.CategoryPath == "Towns / Shops");

        var nameGroup = Assert.Single(names.GetAllGroups());
        Assert.Equal("bird", nameGroup.Type);
        Assert.Equal("a wren", Assert.Single(nameGroup.Names));

        var profession = Assert.Single(professions.GetAllProfessions());
        Assert.Equal("Samurai", profession.Name);
        Assert.Equal("SamuraiTrue", profession.TrueName);
        Assert.Equal(1062948, profession.NameId);
        Assert.Equal(1062950, profession.DescId);
        Assert.Equal(6, profession.Desc);
        Assert.True(profession.TopLevel);
        Assert.Equal(5591, profession.Gump);
        Assert.Equal("Profession", profession.Type);
        var professionSkill = Assert.Single(profession.Skills);
        Assert.Equal("Bushido", professionSkill.Name);
        Assert.Equal(30, professionSkill.Value);
        var professionStat = Assert.Single(profession.Stats);
        Assert.Equal("Str", professionStat.Type);
        Assert.Equal(40, professionStat.Value);

        var signEntries = signs.GetAllEntries();
        Assert.Equal(2, signEntries.Count);
        Assert.Equal([0, 1], signEntries.Select(static entry => entry.MapId).Order());
        Assert.All(signEntries, entry => Assert.Equal(3032, entry.ItemId));

        var decorationEntries = decorations.GetAllEntries();
        Assert.Equal(2, decorationEntries.Count);
        Assert.Equal([0, 1], decorationEntries.Select(static entry => entry.MapId).Order());
        var decoration = decorationEntries[0];
        Assert.Equal("Britannia", decoration.SourceGroup);
        Assert.Equal("sample.yaml", decoration.SourceFile);
        Assert.Equal("Static", decoration.TypeName);
        Assert.Equal("stone wall", decoration.Description);
        Assert.Equal(99, decoration.ItemId);
        Assert.Equal("Hue=0x482", decoration.Parameters["arguments"]);
        Assert.True(decoration.Target.HasValue);
        Assert.Equal(new Point3D(1518, 1671, 21), decoration.Target.Value);
        Assert.Equal("sample note", decoration.Extra);

        Assert.Equal(2, mounts.GetAllEntries().Count);
        Assert.True(mounts.Contains(0x3E90));
        Assert.True(mounts.Contains(16017));
    }

    [Fact]
    public void NameDataService_SetGroups_DefensivelyCopiesInputCollections()
    {
        var names = new List<string>
        {
            "first"
        };
        var groups = new List<NameGroupEntry>
        {
            new("test", names)
        };
        var service = new NameDataService();

        service.SetGroups(groups);
        names.Add("second");
        groups.Clear();

        var loaded = Assert.Single(service.GetAllGroups());
        Assert.Equal("test", loaded.Type);
        Assert.Equal("first", Assert.Single(loaded.Names));
    }

    [Fact]
    public void ProfessionDataService_SetProfessions_DefensivelyCopiesInputCollections()
    {
        var skills = new List<ProfessionSkillEntry>
        {
            new("Bushido", 30)
        };
        var stats = new List<ProfessionStatEntry>
        {
            new("Str", 40)
        };
        var professions = new List<ProfessionEntry>
        {
            new("Samurai", "SamuraiTrue", 1, 2, 3, true, 4, "Profession", skills, stats)
        };
        var service = new ProfessionDataService();

        service.SetProfessions(professions);
        skills.Add(new("Tactics", 20));
        stats.Add(new("Dex", 30));
        professions.Clear();

        var loaded = Assert.Single(service.GetAllProfessions());
        Assert.Equal("Samurai", loaded.Name);
        Assert.Equal("Bushido", Assert.Single(loaded.Skills).Name);
        Assert.Equal("Str", Assert.Single(loaded.Stats).Type);
    }

    private void WriteCatalogYaml()
    {
        WriteFile(
            Path.Combine(_dataDirectory, "teleporters", "nested", "teleporters.yaml"),
            """
            teleporter:
              - src:
                  map: Felucca
                  loc: [10, 20, 0]
                dst:
                  map: Trammel
                  loc: [11, 21, 1]
                back: true
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "regions", "regions.yaml"),
            """
            region:
              - type: TownRegion
                map: Felucca
                name: britain
                priority: 50
                area:
                  - x1: 100
                    y1: 200
                    x2: 110
                    y2: 220
                go_location:
                  x: 1496
                  y: 1628
                  z: 20
                music: Britain
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "weather", "weather.yaml"),
            """
            header:
              title: test
            weather_type:
              - id: 1
                name: Desert
                rainchance: 7
                rainintensity:
                  min: 11
                  max: 22
                raintempdrop: 3
                snowchance: 4
                snowintensity:
                  min: 5
                  max: 6
                snowthreshold: 2
                stormchance: 8
                stormintensity:
                  min: 9
                  max: 10
                stormtempdrop: 12
                maxtemp: 31
                mintemp: 13
                coldchance: 14
                coldintensity: 15
                heatchance: 16
                heatintensity: 17
                lightmin: 18
                lightmax: 19
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "containers", "default_containers.yaml"),
            """
            container:
              - id: backpack
                item_id: 3701
                width: 7
                height: 4
                name: Backpack
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "containers", "containers.yaml"),
            """
            container_layout:
              - gump_id: 60
                bounds: [44, 65, 142, 94]
                drop_sound: 72
                item_ids: [3701]
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "locations", "felucca.yaml"),
            """
            name: Felucca
            locations:
              - name: Top Level
                location: [1, 2, 3]
            categories:
              - name: Towns
                locations:
                  - name: Bank
                    location: [4, 5, 6]
                categories:
                  - name: Shops
                    locations:
                      - name: Bakery
                        location: [7, 8, 9]
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "names", "names.yaml"),
            """
            name_group:
              - type: bird
                names: [a wren]
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "Professions", "professions.yaml"),
            """
            profession:
              - name: Samurai
                true_name: SamuraiTrue
                name_id: 1062948
                desc_id: 1062950
                desc: 6
                top_level: true
                gump: 5591
                type: Profession
                skills:
                  - name: Bushido
                    value: 30
                stats:
                  - type: Str
                    value: 40
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "signs", "signs.yaml"),
            """
            sign:
              - map: 0
                item_id: 3032
                location: [373, 904, -1]
                text: "#1016093"
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "decoration", "Britannia", "sample.yaml"),
            """
            decoration:
              - type: Static
                item_id: 99
                arguments: "Hue=0x482"
                description: stone wall
                placements:
                  - location: [1517, 1670, 20]
                    target: [1518, 1671, 21]
                    note: sample note
            """
        );
        WriteFile(
            Path.Combine(_dataDirectory, "support", "uoconvert.yaml"),
            """
            conversion_section:
              - name: Mounts
                entries:
                  - name: Tiles
                    values: [0x3E90, 16017]
            """
        );
    }

    private static void WriteFile(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);

        Assert.False(string.IsNullOrWhiteSpace(directory));
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, contents);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }
}
