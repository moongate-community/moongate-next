using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Server.Data.Events;
using Moongate.Server.Handlers.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;

namespace Moongate.Server.Extensions.Movement;

/// <summary>DryIoc registration for the live world registry and the movement subsystem.</summary>
public static class MovementContainerExtensions
{
    /// <summary>Registers the world mobile registry, movement services, and lifecycle handlers.</summary>
    public static IContainer AddMoongateMovement(this IContainer container)
    {
        container.Register<IWorldMobileRegistry, WorldMobileRegistry>(Reuse.Singleton);
        container.AddTickEventHandler<PlayerDisconnectedRegistryHandler, PlayerDisconnectedEvent>();

        return container;
    }
}
