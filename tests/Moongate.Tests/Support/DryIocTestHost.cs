using DryIoc;
using Microsoft.Extensions.Hosting;
using Moongate.Abstractions.Internal;

namespace Moongate.Tests.Support;

/// <summary>
/// Test helper mirroring the production DryIoc wiring: Moongate services register natively on a
/// <see cref="Container" />, and the orchestrator is surfaced as the host's single
/// <see cref="IHostedService" />.
/// </summary>
internal static class DryIocTestHost
{
    /// <summary>
    /// Resolves the orchestrator that drives the registered Moongate services, as the host sees it.
    /// </summary>
    public static IHostedService Orchestrator(this IContainer container)
        => container.Resolve<MoongateServiceOrchestrator>();
}
