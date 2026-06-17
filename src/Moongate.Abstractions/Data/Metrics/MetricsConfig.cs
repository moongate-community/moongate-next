namespace Moongate.Abstractions.Data.Metrics;

/// <summary>
///     Configuration for <see cref="Interfaces.Metrics.IMetricsService" />.
/// </summary>
public sealed class MetricsConfig
{
    /// <summary>
    ///     How often the background refresh polls every <see cref="Interfaces.Metrics.IMetricProvider" />.
    ///     Default 5 s.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Enables periodic metrics logging.
    /// </summary>
    public bool LogEnabled { get; set; } = true;

    /// <summary>
    ///     How often the latest metrics snapshot is written to the logger.
    ///     Default 1 min.
    /// </summary>
    public TimeSpan LogInterval { get; set; } = TimeSpan.FromMinutes(1);
}
