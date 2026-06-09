using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;

namespace Moongate.Tests.Persistence;

public class MessagePackSnapshotServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-snap-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    private MessagePackSnapshotService NewService()
        => new(_dir, ".snapshot.bin");

    [Fact]
    public async Task LoadBucket_MissingFile_ReturnsNull()
    {
        Assert.Null(await NewService().LoadBucketAsync("TestPlayer"));
    }

    [Fact]
    public async Task SaveBucket_WritesPerTypeFile_NoTemp()
    {
        var service = NewService();
        var bucket = new EntitySnapshotBucket { TypeId = 1, TypeName = "TestPlayer", SchemaVersion = 1, Payload = [1, 2] };

        await service.SaveBucketAsync(bucket, 9);

        Assert.True(File.Exists(Path.Combine(_dir, "TestPlayer.snapshot.bin")));
        Assert.False(File.Exists(Path.Combine(_dir, "TestPlayer.snapshot.bin.tmp")));
    }

    [Fact]
    public async Task SaveThenLoad_RoundTripsBucketAndSequence()
    {
        var service = NewService();
        var bucket = new EntitySnapshotBucket { TypeId = 1, TypeName = "TestPlayer", SchemaVersion = 1, Payload = [1, 2, 3] };

        await service.SaveBucketAsync(bucket, 9);
        var loaded = await service.LoadBucketAsync("TestPlayer");

        Assert.NotNull(loaded);
        Assert.Equal(9, loaded!.LastSequenceId);
        Assert.Equal("TestPlayer", loaded.Bucket.TypeName);
        Assert.Equal(new byte[] { 1, 2, 3 }, loaded.Bucket.Payload);
    }

    [Fact]
    public async Task LoadBucket_CorruptedFile_ReturnsNull()
    {
        var service = NewService();
        var bucket = new EntitySnapshotBucket { TypeId = 1, TypeName = "TestPlayer", SchemaVersion = 1, Payload = [10, 20, 30] };
        await service.SaveBucketAsync(bucket, 1);

        // Truncate the file so it can no longer be deserialized.
        var path = Path.Combine(_dir, "TestPlayer.snapshot.bin");
        var bytes = await File.ReadAllBytesAsync(path);
        await File.WriteAllBytesAsync(path, bytes[..(bytes.Length / 2)]);

        Assert.Null(await service.LoadBucketAsync("TestPlayer"));
    }
}
