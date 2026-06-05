using System.Globalization;
using System.Text;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Timing;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Background metrics aggregator. Builds a <see cref="MetricsSnapshot" /> every
/// <see cref="MetricsConfig.RefreshInterval" /> from a timer registered on
/// <see cref="ITimerService" />, and serves the cached snapshot on every scrape.
/// </summary>
public sealed class MetricsService : IMetricsService
{
    private const string RefreshTimerName = "metrics-refresh";
    private const string LogTimerName = "metrics-log";

    private readonly ILogger _logger = Log.ForContext<MetricsService>();
    private readonly IReadOnlyList<IMetricProvider> _providers;
    private readonly ITimerService _timer;
    private readonly MetricsConfig _config;

    private MetricsSnapshot _latestSnapshot;
    private string? _logTimerId;
    private string? _timerId;

    public MetricsService(IEnumerable<IMetricProvider> providers, ITimerService timer, MetricsConfig config)
    {
        _providers = providers.ToArray();
        _timer = timer;
        _config = config;
        _latestSnapshot = new(DateTimeOffset.MinValue, Array.Empty<MetricSample>());
    }

    public MetricsSnapshot GetSnapshot()
        => Volatile.Read(ref _latestSnapshot);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RefreshSnapshot();
        _timerId = _timer.RegisterTimer(
            RefreshTimerName,
            _config.RefreshInterval,
            RefreshSnapshot,
            repeat: true
        );

        if (_config.LogEnabled)
        {
            _logTimerId = _timer.RegisterTimer(
                LogTimerName,
                _config.LogInterval,
                LogSnapshot,
                repeat: true
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_logTimerId is not null)
        {
            _timer.UnregisterTimer(_logTimerId);
            _logTimerId = null;
        }

        if (_timerId is not null)
        {
            _timer.UnregisterTimer(_timerId);
            _timerId = null;
        }

        return Task.CompletedTask;
    }

    private void LogSnapshot()
    {
        var snapshot = GetSnapshot();

        if (snapshot.Samples.Count == 0)
        {
            _logger.Information("Metrics snapshot is empty");

            return;
        }

        var values = new StringBuilder(snapshot.Samples.Count * 24);

        for (var i = 0; i < snapshot.Samples.Count; i++)
        {
            if (i > 0)
            {
                values.Append(", ");
            }

            var sample = snapshot.Samples[i];
            values.Append(sample.Name);
            values.Append('=');
            values.Append(sample.Value.ToString(CultureInfo.InvariantCulture));
        }

        _logger.Information(
            "Metrics snapshot at {CollectedAt}: {MetricCount} metrics [{Metrics}]",
            snapshot.CollectedAt,
            snapshot.Samples.Count,
            values.ToString()
        );
    }

    private void RefreshSnapshot()
    {
        var collectedAt = DateTimeOffset.UtcNow;
        var samples = new List<MetricSample>(_providers.Count * 4);

        for (var i = 0; i < _providers.Count; i++)
        {
            var provider = _providers[i];

            try
            {
                var providerSamples = provider.Collect();

                for (var s = 0; s < providerSamples.Count; s++)
                {
                    var sample = providerSamples[s];
                    samples.Add(sample with { Name = $"{provider.Prefix}_{sample.Name}" });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "MetricProvider {Provider} failed to Collect; skipped",
                    provider.GetType().Name
                );
            }
        }

        Volatile.Write(ref _latestSnapshot, new(collectedAt, samples));
    }
}
