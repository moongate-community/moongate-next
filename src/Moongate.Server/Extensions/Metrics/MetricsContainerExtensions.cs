using DryIoc;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Metrics;

namespace Moongate.Server.Extensions.Metrics;

/// <summary>
/// DryIoc-native bootstrap helpers for the Moongate metrics service.
/// </summary>
public static class MetricsContainerExtensions
{
    private const int MetricsServicePriority = 5;

    /// <param name="container">DryIoc container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers <see cref="MetricsService" /> with the Moongate hosting orchestrator.
        /// Requires <c>AddMoongateTimerWheel</c> to have been called earlier so
        /// <see cref="ITimerService" /> is resolvable.
        /// </summary>
        public IContainer AddMoongateMetrics()
        {
            container.AddMoongateHosting();

            container.RegisterConfigSection("metrics", () => new MetricsConfig());

            container.AddMoongateService<IMetricsService, MetricsService>(MetricsServicePriority);

            return container;
        }
    }
}
