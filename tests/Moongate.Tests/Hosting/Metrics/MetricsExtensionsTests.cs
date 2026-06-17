using DryIoc;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Metrics;
using Moongate.Server.Extensions.Timing;

namespace Moongate.Tests.Hosting.Metrics;

public class MetricsExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-metrics-config-{Guid.NewGuid():N}");
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
    public void AddMetricProvider_AliasesAnExistingSingletonAsIMetricProvider()
    {
        var container = new Container();
        container.Register<NamedProvider>(Reuse.Singleton);
        container.AddMetricProvider<NamedProvider>();

        var providers = container.Resolve<IEnumerable<IMetricProvider>>().ToArray();

        Assert.Single(providers);
        Assert.IsType<NamedProvider>(providers[0]);
        Assert.Same(container.Resolve<NamedProvider>(), providers[0]);
    }

    [Fact]
    public void AddMoongateMetrics_AppliesCustomConfig()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "metrics:\n  refresh_interval: \"00:00:02\"\n");

        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateMetrics();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(2), cfg.RefreshInterval);
    }

    [Fact]
    public void AddMoongateMetrics_DefaultConfig_FiveSeconds()
    {
        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateMetrics();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(5), cfg.RefreshInterval);
    }

    [Fact]
    public void AddMoongateMetrics_RegistersServiceAndConfig()
    {
        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateMetrics();
        container.AddMoongateConfig(Path_);

        Assert.NotNull(container.Resolve<IMetricsService>());
        Assert.NotNull(container.Resolve<MetricsConfig>());
    }

    private sealed class NamedProvider : IMetricProvider
    {
        public string Prefix => "named";

        public IReadOnlyList<MetricSample> Collect()
        {
            return [new("v", 1)];
        }
    }
}
