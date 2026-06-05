using Moongate.Abstractions.Data.Metrics;

namespace Moongate.Abstractions.Interfaces.Metrics;

/// <summary>
/// Implemented by services that expose runtime metrics.
/// </summary>
public interface IMetricProvider
{
    /// <summary>Prefix prepended to every sample name returned from <see cref="Collect" />.</summary>
    string Prefix { get; }

    /// <summary>
    /// Collects current samples. Must be cheap and thread-safe.
    /// Sample names returned MUST NOT include the prefix — the collector applies it.
    /// </summary>
    IReadOnlyList<MetricSample> Collect();
}
