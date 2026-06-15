using DryIoc;
using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Jobs;

namespace Moongate.Server.Extensions.Jobs;

/// <summary>DryIoc registration for the job service.</summary>
public static class JobContainerExtensions
{
    private const int JobServicePriority = 4;

    /// <summary>Registers <see cref="JobService" /> with the Moongate hosting orchestrator.</summary>
    public static IContainer AddMoongateJobs(this IContainer container)
    {
        container.AddMoongateHosting();

        container.AddMoongateService<IJobService, JobService>(JobServicePriority);

        return container;
    }
}
