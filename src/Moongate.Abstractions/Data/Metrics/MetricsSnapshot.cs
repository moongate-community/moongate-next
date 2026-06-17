namespace Moongate.Abstractions.Data.Metrics;

/// <summary>
///     Point-in-time aggregation of every <see cref="Interfaces.Metrics.IMetricProvider" />'s samples.
///     Returned by <see cref="Interfaces.Metrics.IMetricsService.GetSnapshot" />.
/// </summary>
public sealed record MetricsSnapshot
{
    public MetricsSnapshot(DateTimeOffset collectedAt, IReadOnlyList<MetricSample> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        CollectedAt = collectedAt;
        Samples = samples;
    }

    /// <summary>Wall-clock time at which the providers were polled.</summary>
    public DateTimeOffset CollectedAt { get; }

    /// <summary>Flat list of samples with provider prefix already applied to each name.</summary>
    public IReadOnlyList<MetricSample> Samples { get; }
}
