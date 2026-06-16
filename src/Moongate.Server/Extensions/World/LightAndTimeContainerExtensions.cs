using DryIoc;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;

namespace Moongate.Server.Extensions.World;

/// <summary>DryIoc registration for the light/time hosted service.</summary>
public static class LightAndTimeContainerExtensions
{
    private const int LightAndTimePriority = 26;

    /// <summary>Registers <see cref="LightAndTimeService" /> with the Moongate hosting orchestrator.</summary>
    public static IContainer AddMoongateLightAndTime(this IContainer container)
    {
        container.AddMoongateHosting();
        container.AddMoongateService<ILightAndTimeService, LightAndTimeService>(LightAndTimePriority);

        return container;
    }
}
