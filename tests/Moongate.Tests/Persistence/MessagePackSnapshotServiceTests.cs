using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;

namespace Moongate.Tests.Persistence;

public class MessagePackSnapshotServiceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"nh-snap-{Guid.NewGuid():N}.bin");

    public void Dispose()
    {
        foreach (var p in new[] { _path, _path + ".tmp" })
        {
            if (File.Exists(p))
            {
                File.Delete(p);
            }
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Load_MissingFile_ReturnsNull()
    {
        var service = new MessagePackSnapshotService(_path);

        Assert.Null(await service.LoadAsync());
    }

    [Fact]
    public async Task Save_LeavesNoTempFile()
    {
        var service = new MessagePackSnapshotService(_path);

        await service.SaveAsync(new() { LastSequenceId = 1 });

        Assert.False(File.Exists(_path + ".tmp"));
    }

    [Fact]
    public async Task SaveThenLoad_RoundTrips()
    {
        var service = new MessagePackSnapshotService(_path);
        var snapshot = new WorldSnapshot
        {
            CreatedUnixMilliseconds = 123,
            LastSequenceId = 9,
            EntityBuckets = [new() { TypeId = 1, TypeName = "TestPlayer", SchemaVersion = 1, Payload = [1, 2] }]
        };

        await service.SaveAsync(snapshot);
        var back = await service.LoadAsync();

        Assert.NotNull(back);
        Assert.Equal(9, back!.LastSequenceId);
        Assert.Single(back.EntityBuckets);
        Assert.Equal("TestPlayer", back.EntityBuckets[0].TypeName);
    }
}
