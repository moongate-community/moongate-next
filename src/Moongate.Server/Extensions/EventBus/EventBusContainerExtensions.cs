using DryIoc;
using Moongate.Abstractions.Data;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.EventBus;
using Moongate.Server.Services.GameLoop;

namespace Moongate.Server.Extensions.EventBus;

/// <summary>
/// DryIoc-native bootstrap helpers for the Moongate event bus + game loop.
/// </summary>
public static class EventBusContainerExtensions
{
    private const int EventBusPriority = 0;
    private const int GameLoopPriority = 10;

    /// <summary>
    /// Registers <see cref="EventBusService" /> and <see cref="GameLoopService" /> with the
    /// Moongate hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateEventBus(this IContainer container)
    {
        container.AddMoongateHosting();

        container.RegisterConfigSection("game_loop", () => new GameLoopConfig());

        container.AddMoongateService<IEventBusService, EventBusService>(EventBusPriority);
        container.AddMoongateService<IGameLoopService, GameLoopService>(GameLoopPriority);

        return container;
    }
}
