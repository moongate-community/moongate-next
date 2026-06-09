using DryIoc;
using Moongate.Abstractions.Data.Logging;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Core.Types;
using Moongate.Server.Data;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Auth;
using Moongate.Server.Extensions.Commands;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Items;
using Moongate.Server.Extensions.Logging;
using Moongate.Server.Extensions.Metrics;
using Moongate.Server.Extensions.Mobiles;
using Moongate.Server.Extensions.Network;
using Moongate.Server.Extensions.Persistence;
using Moongate.Server.Extensions.Plugins;
using Moongate.Server.Extensions.Scripting;
using Moongate.Server.Extensions.Seed;
using Moongate.Server.Extensions.Timing;
using Moongate.Server.Extensions.UoData;
using Moongate.Server.Extensions.Users;
using Moongate.Server.Extensions.WorldData;
using Moongate.Server.FileLoaders;
using Moongate.Server.Interfaces.LiveConsole;
using Moongate.Server.Services.Diagnostics;
using Moongate.Server.Services.EventBus;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.LiveConsole;
using Moongate.Server.Services.Logging;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Timing;
using Serilog;

namespace Moongate.Server.Bootstrap;

/// <summary>
/// Groups the boot-time DryIoc registrations into ordered, named phases so the boot story reads
/// top-to-bottom. The registration order is load-bearing (see each phase's remarks); do not reorder.
/// </summary>
internal static class BootstrapRegistrationExtensions
{
    /// <summary>Persistence, then bundled-asset seeding, then the UO client-file and world stores.</summary>
    public static IContainer AddDataPersistence(this IContainer container, MoongateBootstrapContext context)
    {
        var directories = context.Directories;

        // Persistence (priority 15): snapshot + journal.
        container.AddMoongatePersistence(directories[DirectoryType.Save]);

        // Seed bundled YAML assets from embedded resources, then register client-file + UO stores.
        var dataDirectory = directories[DirectoryType.Data];
        BundledDataAssetsBootstrapper.EnsureDataAssets(dataDirectory, Log.Logger);
        container.AddMoongateUoData(Path.Combine(dataDirectory, "uo_files"));
        container.AddMoongateWorldData(dataDirectory);

        return container;
    }

    /// <summary>UO domain services: they register persisted entities before persistence starts.</summary>
    public static IContainer AddDomainServices(this IContainer container)
    {
        container.AddMoongateUsers();
        container.AddMoongateItems();
        container.AddMoongateMobiles();
        RaceLoader.RegisterDefaultRaces();
        container.AddDefaultAdminUserSeed();
        container.AddMoongateAuth();

        return container;
    }

    /// <summary>Network, packet handlers and commands, Lua scripting, then plugins.</summary>
    public static IContainer AddNetworkAndScripting(this IContainer container, MoongateBootstrapContext context)
    {
        var directories = context.Directories;

        // Network: TCP game listeners + UDP ping server + packet parser (priority 20).
        container.AddMoongateNetwork();
        container.AddMoongatePacketHandlers();
        container.AddMoongateCommands();
        container.AddMetricProvider<NetworkService>();

        // Lua scripting engine (priority 30).
        container.AddMoongateLuaScripting(directories);

        // Plugins can declare config sections, services, Lua modules, persistence entities, and handlers.
        // This must run before AddMoongateConfig so plugin config sections are bound at boot.
        container.AddMoongatePlugins(directories);

        return container;
    }

    /// <summary>
    /// Logging (incl. the live-console broadcaster + relay), event bus + game loop, seeds, timer wheel and metrics
    /// providers.
    /// </summary>
    public static IContainer AddObservability(this IContainer container)
    {
        // Logger config is loaded with the rest of the YAML sections, then applied immediately.
        container.AddMoongateLogging();

        // Live admin console: the broadcaster must exist before the logger is built (the sink feeds
        // it) and is shared by the hub + relay. The relay is a singleton bridged as an IHostedService
        // in AddMoongateAspNetServices.
        container.RegisterInstance<ILiveConsoleBroadcaster>(new LiveConsoleBroadcaster());
        container.Register<LiveConsoleRelay>(Reuse.Singleton);

        // Event bus + game loop (priority 0 / 10) and the diagnostic handler.
        container.AddMoongateEventBus();
        container.AddTickEventHandler<ServerStartedHandler, ServerStartedEvent>();
        container.AddMoongateSeeds();

        // Metrics: needs the timer wheel for the background refresh.
        container.AddMoongateTimerWheel();
        container.AddMoongateMetrics();
        container.AddMetricProvider<EventBusService>();
        container.AddMetricProvider<GameLoopService>();
        container.AddMetricProvider<TimerWheelService>();

        return container;
    }

    /// <summary>Loads the root YAML config (must run last), then builds and assigns the real logger.</summary>
    public static IContainer LoadConfigurationAndLogger(this IContainer container, MoongateBootstrapContext context)
    {
        var directories = context.Directories;

        // Load the root YAML config once and register every section as a DI instance. Must run after
        // all RegisterConfigSection calls (each module helper declares its section).
        container.AddMoongateConfig(RuntimePaths.ResolveConfigPath(directories));
        Log.Logger = LoggerService.CreateLogger(
            container.Resolve<LoggerConfig>(),
            directories[DirectoryType.Logs],
            container.Resolve<ILiveConsoleBroadcaster>()
        );

        return container;
    }

    /// <summary>Registers the boot context instances (directories + packet registry).</summary>
    public static IContainer RegisterBootstrapContext(this IContainer container, MoongateBootstrapContext context)
    {
        container.RegisterInstance(context.Directories, IfAlreadyRegistered.Keep);
        container.RegisterInstance(context.PacketRegistry);

        return container;
    }
}
