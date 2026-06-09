using Moongate.Abstractions.Data.Persistence;
using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence.Service;

public sealed class PerTypeSnapshotTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-pertype-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SaveSnapshot_WritesOneFilePerType_AndReloads()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new() { Id = new(1), Name = "Bob" });
        await first.GetDataAccess<TestItem, Serial>().UpsertAsync(new() { Id = new(Serial.ItemOffset + 1), Label = "Sword" });
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        // One file per registered type — no single mega file.
        Assert.True(File.Exists(Path.Combine(_dir, "TestPlayer.snapshot.bin")));
        Assert.True(File.Exists(Path.Combine(_dir, "TestItem.snapshot.bin")));
        Assert.False(File.Exists(Path.Combine(_dir, "world.snapshot.bin")));

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal("Bob", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(1)))!.Name);
        Assert.Equal("Sword", (await second.GetDataAccess<TestItem, Serial>().GetByIdAsync(new(Serial.ItemOffset + 1)))!.Label);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task OrphanSnapshotFile_ForUnregisteredType_IsIgnored()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new() { Id = new(1), Name = "Bob" });
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        // A snapshot file for a type that is not registered must be ignored on load.
        await File.WriteAllBytesAsync(Path.Combine(_dir, "GhostEntity.snapshot.bin"), [0xDE, 0xAD, 0xBE, 0xEF]);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal("Bob", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(1)))!.Name);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RemovedEntities_StayRemoved_AfterEmptyingTypeAndReload()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var players = first.GetDataAccess<TestPlayer, Serial>();
        await players.UpsertAsync(new() { Id = new(1), Name = "Bob" });
        await first.SaveSnapshotAsync();
        await players.RemoveAsync(new(1));
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        // The emptied type's snapshot file must be gone so it cannot resurrect on reload.
        Assert.False(File.Exists(Path.Combine(_dir, "TestPlayer.snapshot.bin")));

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Null(await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(1)));
        Assert.Equal(0, await second.GetDataAccess<TestPlayer, Serial>().CountAsync());
        await second.StopAsync(CancellationToken.None);
    }

    private PersistenceService NewService()
    {
        Directory.CreateDirectory(_dir);

        var config = new PersistenceConfig { EnableFileLock = false };
        var registrations = new List<PersistenceEntityRegistration>
        {
            new(new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id)),
            new(new PersistenceEntityDescriptor<TestItem, Serial>(2, "TestItem", 1, i => i.Id))
        };

        return new PersistenceService(_dir, config, registrations);
    }
}
