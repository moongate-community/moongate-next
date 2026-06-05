using DryIoc;
using Moongate.Abstractions.Data.Timing;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Timing;

namespace Moongate.Server.Extensions.Timing;

/// <summary>
/// DryIoc-native registration helpers for the Moongate timer wheel.
/// </summary>
public static class TimerContainerExtensions
{
    private const int TimerWheelPriority = 3;

    /// <summary>
    /// Registers <see cref="TimerWheelService" /> with the Moongate hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateTimerWheel(this IContainer container)
    {
        container.AddMoongateHosting();

        container.RegisterConfigSection("timing", () => new TimerWheelConfig());

        container.AddMoongateService<ITimerService, TimerWheelService>(TimerWheelPriority);

        return container;
    }
}
