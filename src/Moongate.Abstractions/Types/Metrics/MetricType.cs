namespace Moongate.Abstractions.Types.Metrics;

/// <summary>
///     Subset of Prometheus / OpenMetrics metric kinds supported in v1.
/// </summary>
public enum MetricType
{
    /// <summary>Cumulative value that only increases. Name in OpenMetrics must end in <c>_total</c>.</summary>
    Counter,

    /// <summary>Instantaneous float that can go up or down.</summary>
    Gauge
}
