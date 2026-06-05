using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Hosting.Metrics;

public class MetricsServiceTests
{
    private sealed class RecordingProvider : IMetricProvider
    {
        private readonly IReadOnlyList<MetricSample> _samples;

        public RecordingProvider(string prefix, IReadOnlyList<MetricSample> samples)
        {
            Prefix = prefix;
            _samples = samples;
        }

        public string Prefix { get; }

        public IReadOnlyList<MetricSample> Collect()
            => _samples;
    }

    private sealed class DynamicProvider : IMetricProvider
    {
        private readonly Func<IReadOnlyList<MetricSample>> _factory;

        public DynamicProvider(string prefix, Func<IReadOnlyList<MetricSample>> factory)
        {
            Prefix = prefix;
            _factory = factory;
        }

        public string Prefix { get; }

        public IReadOnlyList<MetricSample> Collect()
            => _factory();
    }

    private sealed class ThrowingProvider : IMetricProvider
    {
        public ThrowingProvider(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }

        public IReadOnlyList<MetricSample> Collect()
            => throw new InvalidOperationException("boom");
    }

    [Fact]
    public async Task GetSnapshot_BeforeStart_ReturnsEmpty()
    {
        var svc = new MetricsService(Array.Empty<IMetricProvider>(), NewTimer(), new());

        var snapshot = svc.GetSnapshot();

        Assert.Empty(snapshot.Samples);

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RefreshSnapshot_PrependsProviderPrefixToEverySample()
    {
        var p1 = new RecordingProvider("bus", [new("a", 1), new("b", 2, MetricType.Counter)]);
        var p2 = new RecordingProvider("loop", [new("c", 3)]);
        var svc = new MetricsService([p1, p2], NewTimer(), new());

        await svc.StartAsync(CancellationToken.None);

        var names = svc.GetSnapshot().Samples.Select(s => s.Name).ToArray();
        Assert.Contains("bus_a", names);
        Assert.Contains("bus_b", names);
        Assert.Contains("loop_c", names);

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RefreshSnapshot_ProviderThrows_IsSkippedAndOthersContinue()
    {
        var good = new RecordingProvider("good", [new("ok", 1)]);
        var bad = new ThrowingProvider("bad");
        var svc = new MetricsService([bad, good], NewTimer(), new());

        await svc.StartAsync(CancellationToken.None);

        var samples = svc.GetSnapshot().Samples;
        Assert.Single(samples);
        Assert.Equal("good_ok", samples[0].Name);

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RefreshSnapshot_TimerTickRebuildsSnapshot()
    {
        var counter = 0;
        var provider = new DynamicProvider("svc", () => new[] { new MetricSample("n", Interlocked.Increment(ref counter)) });
        var timer = NewTimer();
        var cfg = new MetricsConfig { RefreshInterval = TimeSpan.FromMilliseconds(8) };
        var svc = new MetricsService([provider], timer, cfg);

        await svc.StartAsync(CancellationToken.None);

        // Initial refresh from StartAsync: counter == 1
        Assert.Equal(1, svc.GetSnapshot().Samples.Single().Value);

        // Advance one tick so the repeat timer fires exactly once more.
        timer.UpdateTicksDelta(0);
        timer.UpdateTicksDelta(8); // 8 ms = 1 wheel tick

        Assert.Equal(2, svc.GetSnapshot().Samples.Single().Value);

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_PerformsImmediateRefresh()
    {
        var provider = new RecordingProvider("svc", [new("count", 7)]);
        var svc = new MetricsService([provider], NewTimer(), new());

        await svc.StartAsync(CancellationToken.None);

        var snapshot = svc.GetSnapshot();
        Assert.Single(snapshot.Samples);
        Assert.Equal("svc_count", snapshot.Samples[0].Name);
        Assert.Equal(7, snapshot.Samples[0].Value);

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenMetricsLoggingDisabled_RegistersOnlyRefreshTimer()
    {
        var timer = NewTimer();
        var cfg = new MetricsConfig { LogEnabled = false };
        var svc = new MetricsService(Array.Empty<IMetricProvider>(), timer, cfg);

        await svc.StartAsync(CancellationToken.None);

        Assert.Equal(1, ActiveCount(timer));

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenMetricsLoggingEnabled_RegistersLogTimer()
    {
        var timer = NewTimer();
        var cfg = new MetricsConfig
        {
            LogEnabled = true,
            LogInterval = TimeSpan.FromMilliseconds(16)
        };
        var svc = new MetricsService(Array.Empty<IMetricProvider>(), timer, cfg);

        await svc.StartAsync(CancellationToken.None);

        Assert.Equal(2, ActiveCount(timer));

        await svc.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_UnregistersRefreshTimer()
    {
        var timer = NewTimer();
        var svc = new MetricsService(Array.Empty<IMetricProvider>(), timer, new());

        await svc.StartAsync(CancellationToken.None);
        var activeBefore = ActiveCount(timer);
        await svc.StopAsync(CancellationToken.None);
        var activeAfter = ActiveCount(timer);

        Assert.True(activeBefore >= 1, "expected MetricsService to register at least one timer");
        Assert.Equal(0, activeAfter);
    }

    private static int ActiveCount(TimerWheelService timer)
        => (int)timer.Collect().Single(s => s.Name == "active").Value;

    private static TimerWheelService NewTimer()
        => new(new() { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 64 });
}
