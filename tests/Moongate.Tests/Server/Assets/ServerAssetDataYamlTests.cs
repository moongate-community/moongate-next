using Moongate.Core.Yaml;
using Moongate.UO.Data.Data.ServerAssets;
using YamlDotNet.RepresentationModel;

namespace Moongate.Tests.Server.Assets;

public sealed class ServerAssetDataYamlTests
{
    [Fact]
    public void AssetsData_DoesNotContainLegacyJsonCfgTomlOrTxtFiles()
    {
        var legacyFiles = Directory
            .EnumerateFiles(AssetsDataDirectory(), "*", SearchOption.AllDirectories)
            .Where(path =>
                path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".toml", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
            )
            .Select(path => Path.GetRelativePath(AssetsDataDirectory(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(legacyFiles);
    }

    [Fact]
    public void AssetsData_KnownYamlFilesBindToServerAssetModels()
    {
        var bodies = DeserializeAsset<ServerAssetBodyTable>("bodyTable.yaml");
        var containers = DeserializeAsset<ServerAssetContainerTable>("containers/default_containers.yaml");
        var containerLayouts = DeserializeAsset<ServerAssetContainerLayoutTable>("containers/containers.yaml");
        var conversions = DeserializeAsset<ServerAssetConversionTable>("support/uoconvert.yaml");
        var decorations = DeserializeAsset<ServerAssetDecorationTable>("decoration/Britannia/britain.yaml");
        var doors = DeserializeAsset<ServerAssetDoorTable>("components/doors.yaml");
        var expansions = DeserializeAsset<ServerAssetExpansionTable>("expansions.yaml");
        var locations = DeserializeAsset<ServerAssetMapLocations>("locations/trammel.yaml");
        var names = DeserializeAsset<ServerAssetNameGroupTable>("names/modernuo_names.yaml");
        var professions = DeserializeAsset<ServerAssetProfessionTable>("Professions/professions.yaml");
        var regions = DeserializeAsset<ServerAssetRegionTable>("regions/regions.yaml");
        var signs = DeserializeAsset<ServerAssetSignTable>("signs/signs.yaml");
        var skills = DeserializeAsset<ServerAssetSkillTable>("skills.yaml");
        var spawns = DeserializeAsset<ServerAssetSpawnTable>("spawns/shared/trammel/Vendors.yaml");
        var teleporters = DeserializeAsset<ServerAssetTeleporterTable>("teleporters/teleporters.yaml");
        var weather = DeserializeAsset<ServerAssetWeatherTable>("weather/weather.yaml");

        Assert.NotEmpty(bodies.Body);
        Assert.NotEmpty(containers.Container);
        Assert.NotEmpty(containerLayouts.ContainerLayout);
        Assert.NotEmpty(conversions.ConversionSection);
        Assert.NotEmpty(decorations.Decoration);
        Assert.NotEmpty(doors.Door);
        Assert.NotEmpty(expansions.Expansion);
        Assert.NotEmpty(locations.Categories);
        Assert.NotEmpty(names.NameGroup);
        Assert.NotEmpty(professions.Profession);
        Assert.NotEmpty(regions.Region);
        Assert.NotEmpty(signs.Sign);
        Assert.NotEmpty(skills.Skill);
        Assert.NotEmpty(spawns.Spawn);
        Assert.NotEmpty(teleporters.Teleporter);
        Assert.NotEmpty(weather.WeatherType);
    }

    [Fact]
    public void AssetsData_YamlFilesParse()
    {
        var yamlFiles = Directory
            .EnumerateFiles(AssetsDataDirectory(), "*.yaml", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(yamlFiles);

        foreach (var yamlFile in yamlFiles)
        {
            using var reader = new StringReader(File.ReadAllText(yamlFile));
            var stream = new YamlStream();
            stream.Load(reader);

            Assert.NotEmpty(stream.Documents);
        }
    }

    private static string AssetsDataDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Moongate.Server", "Assets", "data");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/Moongate.Server/Assets/data.");
    }

    private static T DeserializeAsset<T>(string relativePath)
    {
        var path = Path.Combine(AssetsDataDirectory(), relativePath);

        return YamlUtils.DeserializeFromFile<T>(path);
    }
}
