using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Tests.Server.WorldData;

public sealed class ServerAssetDataBootServiceTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-world-data-boot-{Guid.NewGuid():N}");

    [Fact]
    public async Task StartAsync_LoadsRegisteredWorldDataServices()
    {
        Directory.CreateDirectory(Path.Combine(_dataDirectory, "components"));
        File.WriteAllText(
            Path.Combine(_dataDirectory, "components", "doors.yaml"),
            """
            door:
              - category: 3
                pieces: [1705, 1707, 1701, 1703, 1709, 1711, 1713, 1715]
                feature_mask: 0
                comment: Dark Wood Door
            """
        );

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
        var service = new ServerAssetDataBootService(
            new ServerAssetDataLoader(_dataDirectory),
            doors,
            spawns,
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

        await service.StartAsync(CancellationToken.None);

        Assert.Single(doors.GetAllEntries());
        Assert.True(doors.TryGetToggleDefinition(1701, out var definition));
        Assert.Equal(1702, definition.NextItemId);
        Assert.Empty(spawns.GetAllEntries());

        await service.StopAsync(CancellationToken.None);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }
}
