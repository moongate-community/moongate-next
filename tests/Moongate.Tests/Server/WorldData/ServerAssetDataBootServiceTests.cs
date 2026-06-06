using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Tests.Server.WorldData;

public sealed class ServerAssetDataBootServiceTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-world-data-boot-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }

    [Fact]
    public async Task StartAsync_DoesNotForceWorldDataLoading()
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

        var doors = new DoorDataService(new(_dataDirectory));
        var service = new ServerAssetDataBootService();

        await service.StartAsync(CancellationToken.None);

        Assert.False(doors.IsLoaded);

        Assert.Single(doors.GetAllEntries());
        Assert.True(doors.TryGetToggleDefinition(1701, out var definition));
        Assert.Equal(1702, definition.NextItemId);
        Assert.True(doors.IsLoaded);

        await service.StopAsync(CancellationToken.None);
    }
}
