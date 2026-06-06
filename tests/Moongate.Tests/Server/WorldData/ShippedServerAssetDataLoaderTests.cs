using Moongate.Server.Bootstrap;
using Moongate.Server.Data.World;
using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;
using Serilog;

namespace Moongate.Tests.Server.WorldData;

public sealed class ShippedServerAssetDataLoaderTests : IDisposable
{
    private readonly DirectoryInfo _dataDirectory;

    public ShippedServerAssetDataLoaderTests()
    {
        _dataDirectory = Directory.CreateTempSubdirectory("mg-shipped-world-data-");
        BundledDataAssetsBootstrapper.EnsureDataAssets(_dataDirectory.FullName, Log.Logger);
    }

    public void Dispose()
    {
        if (_dataDirectory.Exists)
        {
            _dataDirectory.Delete(true);
        }
    }

    [Fact]
    public void LoadAllShippedWorldData_LoadsEveryFamily()
    {
        var loader = new ServerAssetDataLoader(_dataDirectory.FullName);
        var doors = new DoorDataService();
        var spawns = new SpawnsDataService();
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

        loader.LoadDoors(doors);
        loader.LoadSpawns(spawns);
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

        Assert.NotEmpty(doors.GetAllEntries());
        Assert.NotEmpty(spawns.GetAllEntries());
        Assert.NotEmpty(teleporters.GetAllEntries());
        Assert.NotEmpty(regions.GetAllEntries());
        Assert.NotEmpty(weather.GetAllEntries());
        Assert.NotEmpty(containers.GetAllContainers());
        Assert.NotEmpty(containers.GetAllLayouts());
        Assert.NotEmpty(locations.GetAllLocations());
        Assert.NotEmpty(names.GetAllGroups());
        Assert.NotEmpty(professions.GetAllProfessions());
        Assert.NotEmpty(signs.GetAllEntries());
        Assert.NotEmpty(decorations.GetAllEntries());
        Assert.NotEmpty(mounts.GetAllEntries());
    }

    [Fact]
    public void LoadAllShippedWorldData_PreservesQualityFixEdgeData()
    {
        var loader = new ServerAssetDataLoader(_dataDirectory.FullName);
        var doors = new DoorDataService();
        var spawns = new SpawnsDataService();
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

        loader.LoadDoors(doors);
        loader.LoadSpawns(spawns);
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

        Assert.Contains(regions.GetAllEntries(), entry => entry.Area.Count > 0);
        Assert.Contains(
            weather.GetAllEntries(),
            entry =>
                entry.Name == "Desert" &&
                entry.RainChance == 1 &&
                entry.RainIntensity == new WeatherRange(5, 10) &&
                entry.MaxTemperature == 30 &&
                entry.MinTemperature == 10 &&
                entry.HeatChance == 80 &&
                entry.HeatIntensity == 35 &&
                entry.LightMin == 0 &&
                entry.LightMax == 5
        );
        Assert.Contains(
            decorations.GetAllEntries(),
            entry =>
                entry.SourceGroup == "Malas" &&
                entry.SourceFile == "markcontainers.yaml" &&
                entry.TypeName == "MarkContainer" &&
                entry.Target.HasValue &&
                !string.IsNullOrWhiteSpace(entry.Description)
        );

        var sourceMapZeroSigns = signs
                                 .GetAllEntries()
                                 .Where(static entry => entry.SourceMapCode == 0)
                                 .ToArray();

        Assert.NotEmpty(sourceMapZeroSigns);
        Assert.Contains(
            sourceMapZeroSigns.GroupBy(static entry => new { entry.ItemId, entry.Location, entry.Text }),
            group => group.Any(static entry => entry.MapId == 0) && group.Any(static entry => entry.MapId == 1)
        );
    }
}
