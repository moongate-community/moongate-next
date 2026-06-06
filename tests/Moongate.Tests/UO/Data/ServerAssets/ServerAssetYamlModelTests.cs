using Moongate.Core.Yaml;
using Moongate.UO.Data.Data.ServerAssets;

namespace Moongate.Tests.UO.Data.ServerAssets;

public sealed class ServerAssetYamlModelTests
{
    [Fact]
    public void Deserialize_BodyTable_BindsRows()
    {
        const string Yaml =
            """
            body:
              - id: 400
                type: Human
            """;

        var table = YamlUtils.Deserialize<ServerAssetBodyTable>(Yaml);

        Assert.Equal(400, Assert.Single(table.Body).Id);
    }

    [Fact]
    public void Deserialize_ContainerLayout_BindsBoundsAndItemIds()
    {
        const string Yaml =
            """
            container_layout:
              - gump_id: 60
                bounds: [44, 65, 142, 94]
                drop_sound: 72
                item_ids: [3701, 3702]
            """;

        var table = YamlUtils.Deserialize<ServerAssetContainerLayoutTable>(Yaml);

        var layout = Assert.Single(table.ContainerLayout);
        Assert.Equal([44, 65, 142, 94], layout.Bounds);
        Assert.Equal([3701, 3702], layout.ItemIds);
    }

    [Fact]
    public void Deserialize_Containers_BindsDefaults()
    {
        const string Yaml =
            """
            container:
              - id: backpack
                item_id: 3701
                width: 7
                height: 4
                name: Backpack
            """;

        var table = YamlUtils.Deserialize<ServerAssetContainerTable>(Yaml);

        Assert.Equal("backpack", Assert.Single(table.Container).Id);
    }

    [Fact]
    public void Deserialize_Conversion_BindsSections()
    {
        const string Yaml =
            """
            conversion_section:
              - name: StaticOptions
                entries:
                  - name: MaxStaticsPerBlock
                    values: ["1000"]
            """;

        var table = YamlUtils.Deserialize<ServerAssetConversionTable>(Yaml);

        Assert.Equal("StaticOptions", Assert.Single(table.ConversionSection).Name);
    }

    [Fact]
    public void Deserialize_Decoration_BindsPlacementNotesAndTargets()
    {
        const string Yaml =
            """
            decoration:
              - type: MetalChest
                item_id: 2475
                arguments: ""
                description: metal chest
                placements:
                  - location: [1653, 1600, 26]
                    target: [1006, 994, -70]
                    note: spawning
            """;

        var table = YamlUtils.Deserialize<ServerAssetDecorationTable>(Yaml);

        var placement = Assert.Single(Assert.Single(table.Decoration).Placements);
        Assert.Equal([1653, 1600, 26], placement.Location);
        Assert.Equal([1006, 994, -70], placement.Target);
        Assert.Equal("spawning", placement.Note);
    }

    [Fact]
    public void Deserialize_DoorTable_BindsPieces()
    {
        const string Yaml =
            """
            door:
              - category: 0
                pieces: [1657, 1659, 1653, 1655, 1661, 1663, 1665, 1667]
                feature_mask: 0
                comment: Metal Door
            """;

        var table = YamlUtils.Deserialize<ServerAssetDoorTable>(Yaml);

        Assert.Equal(8, Assert.Single(table.Door).Pieces.Count);
    }

    [Fact]
    public void Deserialize_ExpansionTable_BindsFlagDictionaries()
    {
        const string Yaml =
            """
            expansion:
              - id: 5
                name: Age of Shadows
                required_client:
                client_flags: Malas
                supported_features:
                  aos: true
                map_selection_flags:
                  malas: true
                character_list_flags:
                  aos: true
                housing_flags:
                  aos: true
                mobile_status_version: 6
            """;

        var table = YamlUtils.Deserialize<ServerAssetExpansionTable>(Yaml);

        Assert.True(Assert.Single(table.Expansion).SupportedFeatures["aos"]);
    }

    [Fact]
    public void Deserialize_Locations_BindsNestedCategories()
    {
        const string Yaml =
            """
            name: Trammel
            categories:
              - name: Dungeons
                locations:
                  - name: Covetous
                    location: [2499, 919, 0]
            """;

        var locations = YamlUtils.Deserialize<ServerAssetMapLocations>(Yaml);

        Assert.Equal("Covetous", locations.Categories[0].Locations[0].Name);
    }

    [Fact]
    public void Deserialize_NameGroups_BindsNames()
    {
        const string Yaml =
            """
            name_group:
              - type: bird
                names: [a wren, a swallow]
            """;

        var table = YamlUtils.Deserialize<ServerAssetNameGroupTable>(Yaml);

        Assert.Equal("bird", Assert.Single(table.NameGroup).Type);
    }

    [Fact]
    public void Deserialize_Professions_BindsSkillsAndStats()
    {
        const string Yaml =
            """
            profession:
              - name: Samurai
                true_name: Samurai
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
            """;

        var table = YamlUtils.Deserialize<ServerAssetProfessionTable>(Yaml);

        Assert.Equal("Bushido", Assert.Single(table.Profession).Skills[0].Name);
    }

    [Fact]
    public void Deserialize_Regions_BindsAreasAndLocations()
    {
        const string Yaml =
            """
            region:
              - type: TownRegion
                map: Felucca
                name: The Heartwood
                priority: 50
                area:
                  - x1: 6911
                    y1: 255
                    x2: 7168
                    y2: 512
                entrance:
                  x: 535
                  y: 995
                  z: 0
                go_location:
                  x: 6984
                  y: 337
                  z: 0
                music: ElfCity
            """;

        var table = YamlUtils.Deserialize<ServerAssetRegionTable>(Yaml);

        Assert.Equal("ElfCity", Assert.Single(table.Region).Music);
    }

    [Fact]
    public void Deserialize_Signs_BindsText()
    {
        const string Yaml =
            """
            sign:
              - map: 0
                item_id: 3032
                location: [373, 904, -1]
                text: "#1016093"
            """;

        var table = YamlUtils.Deserialize<ServerAssetSignTable>(Yaml);

        Assert.Equal("#1016093", Assert.Single(table.Sign).Text);
    }

    [Fact]
    public void Deserialize_Skills_BindsStats()
    {
        const string Yaml =
            """
            skill:
              - skill_id: 0
                name: Alchemy
                title: Alchemist
                str_scale: 0.0
                dex_scale: 0.05
                int_scale: 0.05
                stat_total: 10.0
                str_gain: 0.0
                dex_gain: 0.5
                int_gain: 0.5
                gain_factor: 1.0
                profession_skill_name: Alchemy
                primary_stat: Int
                secondary_stat: Dex
            """;

        var table = YamlUtils.Deserialize<ServerAssetSkillTable>(Yaml);

        Assert.Equal(0.5, Assert.Single(table.Skill).DexGain);
    }

    [Fact]
    public void Deserialize_Spawns_BindsEntriesAndDelays()
    {
        const string Yaml =
            """
            spawn:
              - type: Spawner
                guid: 001a5320-820c-4300-96f9-676e428b55be
                name: Spawner (213)
                location: [4066, 569, 0]
                map: Felucca
                count: 8
                min_delay: 00:20:00
                max_delay: 00:20:00
                team: 0
                home_range: 80
                walking_range: 80
                entries:
                  - name: PolarBear
                    max_count: 8
                    probability: 100
            """;

        var table = YamlUtils.Deserialize<ServerAssetSpawnTable>(Yaml);

        Assert.Equal(TimeSpan.FromMinutes(20), Assert.Single(table.Spawn).MinDelay);
        Assert.Equal("PolarBear", table.Spawn[0].Entries[0].Name);
    }

    [Fact]
    public void Deserialize_Teleporters_BindsEndpoints()
    {
        const string Yaml =
            """
            teleporter:
              - src:
                  map: Felucca
                  loc: [311, 786, -24]
                dst:
                  map: Felucca
                  loc: [314, 784, 0]
                back: false
            """;

        var table = YamlUtils.Deserialize<ServerAssetTeleporterTable>(Yaml);

        Assert.Equal(-24, Assert.Single(table.Teleporter).Src.Loc[2]);
    }

    [Fact]
    public void Deserialize_Weather_BindsRanges()
    {
        const string Yaml =
            """
            header:
              title: UOX3 DFNs
              repository: https://github.com/UOX3DevTeam/UOX3
              last_update: 1/4/2003
              script: weatherab.dfn
              description: Weather definitions
            weather_type:
              - id: 1
                rainchance: 1
                rainintensity:
                  min: 5
                  max: 10
                raintempdrop: 5
                snowchance: 0
                snowintensity:
                  min: 0
                  max: 0
                snowthreshold: 0
                stormchance: 0
                stormintensity:
                  min: 0
                  max: 0
                stormtempdrop: 10
                maxtemp: 30
                mintemp: 10
                coldchance: 0
                coldintensity: 0
                heatchance: 80
                heatintensity: 35
                lightmin: 0
                lightmax: 5
                name: Desert
            """;

        var table = YamlUtils.Deserialize<ServerAssetWeatherTable>(Yaml);

        Assert.Equal(10, Assert.Single(table.WeatherType).Rainintensity.Max);
    }
}
