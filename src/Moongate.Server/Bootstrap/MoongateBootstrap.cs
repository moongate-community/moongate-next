using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Moongate.Abstractions.Data.Logging;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Internal;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Network.UO.Registry;
using Moongate.Server.Bootstrap.Internal;
using Moongate.Server.Data;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Logging;
using Moongate.Server.Extensions.Metrics;
using Moongate.Server.Extensions.Network;
using Moongate.Server.Extensions.Persistence;
using Moongate.Server.Extensions.Plugins;
using Moongate.Server.Extensions.Scripting;
using Moongate.Server.Extensions.Seed;
using Moongate.Server.Extensions.Timing;
using Moongate.Server.Extensions.UoData;
using Moongate.Server.Extensions.Users;
using Moongate.Server.Services.Diagnostics;
using Moongate.Server.Services.EventBus;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Logging;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Timing;
using Serilog;

namespace Moongate.Server.Bootstrap;

public static class MoongateBootstrap
{
    public static WebApplication Build(MoongateBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        var app = builder.Build();
        ConfigurePipeline(app);

        return app;
    }

    public static async Task RunAsync(MoongateBootstrapOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        using var pidFileGuard = PidFileGuard.Acquire(context.Directories);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        await using var app = builder.Build();
        ConfigurePipeline(app);

        await app.RunAsync(cancellationToken);
    }

    internal static void ConfigureContainer(IContainer container, MoongateBootstrapContext context)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(context);

        var directories = context.Directories;

        container.RegisterInstance(context.PacketRegistry);

        // Logger config is loaded with the rest of the TOML sections, then applied immediately.
        container.AddMoongateLogging();

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

        // UO domain services register persisted entities before persistence starts.
        container.AddMoongateUsers();
        container.AddDefaultAdminUserSeed();

        // Persistence (priority 15): snapshot + journal.
        container.AddMoongatePersistence(directories[DirectoryType.Save]);

        // UO static data: seed bundled reference data, then register client-file + reference stores.
        UoDataAssetsBootstrapper.EnsureDataAssets(
            Path.Combine(AppContext.BaseDirectory, "Assets", "uo_files"),
            directories[DirectoryType.Data],
            Log.Logger
        );
        container.AddMoongateUoData(directories[DirectoryType.Data]);

        // Network: TCP game listeners + UDP ping server + packet parser (priority 20).
        container.AddMoongateNetwork();
        container.AddMoongatePacketHandlers();
        container.AddMetricProvider<NetworkService>();

        // Lua scripting engine (priority 30).
        container.AddMoongateLuaScripting(directories);

        // Plugins can declare config sections, services, Lua modules, persistence entities, and handlers.
        // This must run before AddMoongateConfig so plugin config sections are bound at boot.
        container.AddMoongatePlugins(directories);

        // Load the root TOML config once and register every section as a DI instance. Must run after
        // all RegisterConfigSection calls (each module helper declares its section).
        container.AddMoongateConfig(RuntimePaths.ResolveConfigPath(directories));
        Log.Logger = LoggerService.CreateLogger(
            container.Resolve<LoggerConfig>(),
            directories[DirectoryType.Logs]
        );
    }

    internal static WebApplicationBuilder CreateBuilder(
        MoongateBootstrapOptions options,
        MoongateBootstrapContext context
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                Args = options.Args,
                EnvironmentName = options.Debug ? Environments.Development : null
            }
        );

        // Back the whole host (REST included) with DryIoc so the Lua scripting engine can
        // register and resolve script-module types at runtime, which MEDI cannot do.
        builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory());

        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        builder.Logging.ClearProviders().AddSerilog();

        // ASP.NET Core services (OpenAPI, Kestrel, routing, ...) register through IServiceCollection.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // The generic host collects hosted services from IServiceCollection, so bridge the
        // DryIoc-registered orchestrator here; it resolves its descriptors from the unified provider.
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MoongateServiceOrchestrator>());

        Log.Information("Registered {PacketCount} UO packets", context.RegisteredPacketCount);

        // Every Moongate service registers natively on the DryIoc container (ASP.NET stays on MEDI,
        // which DryIoc imports). This runs after the IServiceCollection descriptors are populated.
        builder.Host.ConfigureContainer<IContainer>(container => ConfigureContainer(container, context));

        return builder;
    }

    internal static MoongateBootstrapContext CreateContext(MoongateBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rootDirectory = RuntimePaths.ResolveRootDirectory(options.RootDirectory);
        var directories = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());
        var packetRegistry = new PacketRegistry();
        var registeredPacketCount = PacketTable.Register(packetRegistry);

        return new(directories, packetRegistry, registeredPacketCount);
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Publish a tick event the moment the host is up; the handler logs the thread it runs on.
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(
            () =>
            {
                var bus = app.Services.GetRequiredService<IEventBusService>();
                bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));
            }
        );

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapMoongateApiDocs();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapMoongateVersion();
        app.MapMoongateMetrics();
        app.MapFallbackToFile("index.html");
    }
}
