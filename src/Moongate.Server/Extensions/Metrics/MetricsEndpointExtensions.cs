using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Server.Services.Metrics;

namespace Moongate.Server.Extensions.Metrics;

public static class MetricsEndpointExtensions
{
    public static IEndpointConventionBuilder MapMoongateMetrics(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/metrics"
    )
    {
        return endpoints.MapGet(
                pattern,
                (IMetricsService metrics) => Results.Text(
                    OpenMetricsFormatter.Format(metrics.GetSnapshot()),
                    "text/plain; charset=utf-8"
                )
            )
            .WithName("GetMetrics");
    }
}
