using DryIoc;
using Moongate.Abstractions.Data;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Tests.Hosting.EventBus.Support;

namespace Moongate.Tests.Hosting.EventBus;

public class EventBusServiceCollectionExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-eventbus-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "moongate.yaml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsyncEventHandler_RegistersHandlerAndItIsInvokedByBus()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddMoongateEventBus();
        container.AddMoongateConfig(Path_);
        container.AddAsyncEventHandler<NamedAsyncHandler, TestAsyncEvent>();

        await container.Resolve<IEventBusService>().PublishAsync(new TestAsyncEvent("hello"));

        Assert.Equal(new[] { "async:Named:hello" }, timeline);
    }

    [Fact]
    public void AddMoongateEventBus_CustomConfig_AppliesConfig()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "game_loop:\n  idle_sleep_ms: 7\n  idle_cpu_enabled: false\n");

        var container = new Container();

        container.AddMoongateEventBus();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<GameLoopConfig>();

        Assert.Equal(7, cfg.IdleSleepMs);
        Assert.False(cfg.IdleCpuEnabled);
    }

    [Fact]
    public void AddMoongateEventBus_NoCustomConfig_AppliesDefaults()
    {
        var container = new Container();

        container.AddMoongateEventBus();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<GameLoopConfig>();

        Assert.True(cfg.IdleCpuEnabled);
        Assert.Equal(1, cfg.IdleSleepMs);
    }

    [Fact]
    public void AddMoongateEventBus_RegistersBusAndGameLoop()
    {
        var container = new Container();

        container.AddMoongateEventBus();
        container.AddMoongateConfig(Path_);

        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<IGameLoopService>());
    }

    [Fact]
    public void AddTickEventHandler_RegistersHandlerAndDrainInvokesIt()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddMoongateEventBus();
        container.AddMoongateConfig(Path_);
        container.AddTickEventHandler<NamedTickHandler, TestTickEvent>();

        var bus = container.Resolve<IEventBusService>();
        bus.Publish(new TestTickEvent(11));

        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new[] { "tick:Named:11" }, timeline);
    }

    private sealed class NamedAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
    {
        private readonly List<string> _timeline;

        public NamedAsyncHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
        {
            lock (_timeline)
            {
                _timeline.Add($"async:Named:{evt.Payload}");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class NamedTickHandler : ITickEventHandler<TestTickEvent>
    {
        private readonly List<string> _timeline;

        public NamedTickHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public void Handle(TestTickEvent evt)
        {
            lock (_timeline)
            {
                _timeline.Add($"tick:Named:{evt.Value}");
            }
        }
    }
}
