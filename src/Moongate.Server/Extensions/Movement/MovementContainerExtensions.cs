using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Handlers.World;
using Moongate.Server.Interfaces.Services.Movement;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.Movement;
using Moongate.Server.Services.World;

namespace Moongate.Server.Extensions.Movement;

/// <summary>DryIoc registration for the live world registry and the movement subsystem.</summary>
public static class MovementContainerExtensions
{
    private const int WorldEntitiesBootPriority = 18;

    /// <summary>Registers the world mobile registry, movement services, and lifecycle handlers.</summary>
    public static IContainer AddMoongateMovement(this IContainer container)
    {
        container.Register<IWorldSpatialIndex, WorldSpatialIndex>(Reuse.Singleton);
        container.Register<IInterestManagementService, InterestManagementService>(Reuse.Singleton);
        container.Register<IMovementTileQueryService, MovementTileQueryService>(Reuse.Singleton);
        container.Register<IMovementValidationService, MovementValidationService>(Reuse.Singleton);
        container.AddTickEventHandler<PlayerDisconnectedRegistryHandler, PlayerDisconnectedEvent>();
        container.AddTickEventHandler<PlayerDisconnectedInterestHandler, PlayerDisconnectedEvent>();
        container.AddAsyncEventHandler<MovementBroadcastHandler, MobileMovedEvent>();

        container.AddMoongateService<WorldEntitiesBootService>(WorldEntitiesBootPriority);

        return container;
    }
}
