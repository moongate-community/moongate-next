using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Metrics;
using Moongate.Server.Extensions.Timing;
using Moongate.Server.Services.EventBus;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Timing;
using Moongate.Tests.Support;

namespace Moongate.Tests.Hosting.Metrics;

public class MetricsIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-metrics-integration-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "moongate.toml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_AllProvidersAggregatedAndFormatted()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[metrics]\nrefresh_interval = \"00:00:00.0500000\"\n");

        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateTimerWheel();
        container.AddMoongateMetrics();
        container.AddMoongateConfig(Path_);

        container.AddMetricProvider<EventBusService>();
        container.AddMetricProvider<GameLoopService>();
        container.AddMetricProvider<TimerWheelService>();

        var orchestrator = container.Orchestrator();
        var metrics = container.Resolve<IMetricsService>();

        await orchestrator.StartAsync(CancellationToken.None);

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline && metrics.GetSnapshot().Samples.Count == 0)
        {
            await Task.Delay(20);
        }

        var snapshot = metrics.GetSnapshot();
        var names = snapshot.Samples.Select(s => s.Name).ToHashSet();

        Assert.Contains("bus_tick_queue_depth", names);
        Assert.Contains("gameloop_tick_count", names);
        Assert.Contains("timer_active", names);

        var text = OpenMetricsFormatter.Format(snapshot);
        Assert.Contains("# TYPE bus_tick_queue_depth gauge", text);
        Assert.Contains("# TYPE gameloop_tick_count_total counter", text);
        Assert.EndsWith("# EOF\n", text);

        await orchestrator.StopAsync(CancellationToken.None);
    }
}
