using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Tests.Server.WorldData;

public sealed class DoorDataLoaderTests : IDisposable
{
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), $"mg-door-loader-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }

    [Fact]
    public void LoadDoors_MissingDoorYaml_ClearsEntries()
    {
        var service = new DoorDataService();
        service.SetEntries(
            [
                new(
                    0,
                    1701,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    "Existing Door"
                )
            ]
        );
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadDoors(service);

        Assert.Empty(service.GetAllEntries());
        Assert.False(service.TryGetToggleDefinition(1701, out _));
    }

    [Fact]
    public void LoadDoors_WithDoorYaml_LoadsEntriesAndToggleDefinitions()
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
        var service = new DoorDataService();
        var loader = new ServerAssetDataLoader(_dataDirectory);

        loader.LoadDoors(service);

        Assert.Single(service.GetAllEntries());
        Assert.True(service.TryGetToggleDefinition(1701, out var definition));
        Assert.Equal(1702, definition.NextItemId);
        Assert.True(definition.IsClosed);
    }
}
