using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Interfaces.Metrics;

/// <summary>
/// Central service that aggregates samples from every registered <see cref="IMetricProvider" />
/// and exposes the latest snapshot for scraping.
/// </summary>
public interface IMetricsService : IMoongateService
{
    /// <summary>
    /// Returns the most recently built snapshot. O(1) read backed by <see cref="Volatile" />.
    /// </summary>
    MetricsSnapshot GetSnapshot();
}
