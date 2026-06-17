using DryIoc;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Network.UO.Registry;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Player;

namespace Moongate.Server.Extensions.Network;

/// <summary>
///     DryIoc-native registration helpers for the Moongate network service.
/// </summary>
public static class NetworkContainerExtensions
{
    private const int NetworkServicePriority = 20;

    /// <summary>
    ///     Registers <see cref="NetworkService" /> and <see cref="SessionService" /> with the Moongate
    ///     hosting orchestrator. Requires a <see cref="PacketRegistry" /> singleton
    ///     to have been registered earlier.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateNetwork(this IContainer container)
    {
        container.AddMoongateHosting();

        container.RegisterConfigSection("network", () => new NetworkConfig());

        container.RegisterDelegate(
            resolver => new SessionService(resolver.Resolve<IEventBusService>(IfUnresolved.ReturnDefault)),
            Reuse.Singleton
        );
        container.RegisterMapping<ISessionService, SessionService>();
        container.RegisterMapping<INetworkSessionManager, SessionService>();
        container.Register<PlayerSessionService>(Reuse.Singleton);
        container.RegisterMapping<IPlayerSessionService, PlayerSessionService>();
        container.RegisterMapping<ITickEventHandler<PlayerConnectedEvent>, PlayerSessionService>();
        container.RegisterMapping<ITickEventHandler<PlayerDisconnectedEvent>, PlayerSessionService>();
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.AddMoongateService<INetworkService, NetworkService>(NetworkServicePriority);

        return container;
    }
}
