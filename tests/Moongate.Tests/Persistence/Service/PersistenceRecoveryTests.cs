using Moongate.Abstractions.Data.Persistence;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Data.Events;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence.Service;

public class PersistenceRecoveryTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-persist-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Recovery_FromJournalOnly_RebuildsBothEntityTypes()
    {
        var itemId = new Serial(Serial.ItemOffset + 1);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new TestPlayer { Id = new Serial(1), Name = "Bob" });
        await first.GetDataAccess<TestItem, Serial>().UpsertAsync(new TestItem { Id = itemId, Label = "Sword" });
        await first.StopWithoutSnapshotAsync();

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal(1, await second.GetDataAccess<TestPlayer, Serial>().CountAsync());
        Assert.Equal("Bob", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new Serial(1)))!.Name);
        Assert.Equal("Sword", (await second.GetDataAccess<TestItem, Serial>().GetByIdAsync(itemId))!.Label);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Recovery_FromSnapshot_RebuildsStateAndTrimsJournal()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new TestPlayer { Id = new Serial(5), Name = "Snap" });
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal("Snap", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new Serial(5)))!.Name);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Recovery_SnapshotPlusNewerJournal_AppliesBoth()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var players = first.GetDataAccess<TestPlayer, Serial>();
        await players.UpsertAsync(new TestPlayer { Id = new Serial(1), Name = "InSnapshot" });
        await first.SaveSnapshotAsync();
        await players.UpsertAsync(new TestPlayer { Id = new Serial(2), Name = "AfterSnapshot" });
        await first.StopWithoutSnapshotAsync();

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal(2, await second.GetDataAccess<TestPlayer, Serial>().CountAsync());
        Assert.Equal("AfterSnapshot", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new Serial(2)))!.Name);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task SaveSnapshotAsync_PublishesStartedAndCompletedEvents()
    {
        var bus = new CapturingEventBusService();
        var service = NewService(bus);
        await service.StartAsync(CancellationToken.None);
        await service.GetDataAccess<TestPlayer, Serial>()
            .UpsertAsync(new TestPlayer { Id = new Serial(1), Name = "Evented" });

        await service.SaveSnapshotAsync();

        var started = Assert.IsType<SnapshotSaveStartedEvent>(bus.AsyncEvents[0]);
        var completed = Assert.IsType<SnapshotSaveCompletedEvent>(bus.AsyncEvents[1]);
        Assert.True(completed.At >= started.At);
        Assert.Equal(started.At, completed.StartedAt);
        Assert.Equal(1, completed.LastSequenceId);
        Assert.Equal(1, completed.EntityBucketCount);

        await service.StopWithoutSnapshotAsync();
    }

    private PersistenceService NewService(IEventBusService? eventBus = null)
    {
        var config = new PersistenceConfig { EnableFileLock = false };
        var registrations = new List<PersistenceEntityRegistration>
        {
            new(new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id)),
            new(new PersistenceEntityDescriptor<TestItem, Serial>(2, "TestItem", 1, i => i.Id))
        };

        return new PersistenceService(_dir, config, registrations, eventBus: eventBus);
    }

    private sealed class CapturingEventBusService : IEventBusService
    {
        public List<IAsyncEvent> AsyncEvents { get; } = [];
        public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }
        public int CurrentTickQueueDepth => 0;

        public int DrainTickEvents(int maxItems)
        {
            return 0;
        }

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent
        {
        }

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
        {
            AsyncEvents.Add(evt);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
