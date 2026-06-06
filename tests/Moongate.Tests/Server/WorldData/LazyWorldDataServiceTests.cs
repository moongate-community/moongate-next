using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Tests.Server.WorldData;

public sealed class LazyWorldDataServiceTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-lazy-world-data-{Guid.NewGuid():N}");

    [Fact]
    public void SpawnsDataService_GetAllEntries_LoadsSpawnsOnlyOnFirstQuery()
    {
        WriteSpawnYaml("first");
        var service = new SpawnsDataService(new ServerAssetDataLoader(_dataDirectory));

        Assert.True(service.IsLazy);
        Assert.False(service.IsLoaded);

        var firstLoad = service.GetAllEntries();
        WriteSpawnYaml("second");
        var secondLoad = service.GetAllEntries();

        Assert.True(service.IsLoaded);
        Assert.Equal("first", Assert.Single(firstLoad).Name);
        Assert.Equal("first", Assert.Single(secondLoad).Name);
    }

    [Fact]
    public void SpawnsDataService_Reload_ReplacesCachedEntries()
    {
        WriteSpawnYaml("first");
        var service = new SpawnsDataService(new ServerAssetDataLoader(_dataDirectory));

        Assert.Equal("first", Assert.Single(service.GetAllEntries()).Name);
        WriteSpawnYaml("second");

        service.Reload();

        Assert.Equal("second", Assert.Single(service.GetAllEntries()).Name);
    }

    [Fact]
    public async Task ServerAssetDataBootService_StartAsync_DoesNotLoadWorldDataServices()
    {
        WriteSpawnYaml("first");
        var service = new ServerAssetDataBootService();
        var spawns = new SpawnsDataService(new ServerAssetDataLoader(_dataDirectory));

        await service.StartAsync(CancellationToken.None);

        Assert.False(spawns.IsLoaded);
    }

    [Fact]
    public void SpawnsDataService_ImplementsCommonDataServiceContract()
    {
        var service = new SpawnsDataService(new ServerAssetDataLoader(_dataDirectory));

        Assert.IsAssignableFrom<IDataService>(service);
        Assert.True(service.IsLazy);
    }

    private void WriteSpawnYaml(string name)
    {
        WriteFile(
            Path.Combine(_dataDirectory, "spawns", "shared", "felucca", "Test.yaml"),
            $$"""
            spawn:
              - guid: 11111111-1111-1111-1111-111111111111
                type: Spawner
                name: {{name}}
                location: [100, 200, 0]
                count: 1
                min_delay: 00:01:00
                max_delay: 00:02:00
                team: 0
                home_range: 4
                walking_range: 6
                entries:
                  - name: mongbat
                    max_count: 1
                    probability: 100
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
