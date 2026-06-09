using System.Diagnostics;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Network.UO.Registry;
using Moongate.Server.Bootstrap.Internal;
using Moongate.Server.Data;
using Moongate.Server.Services.Diagnostics;
using Serilog;

namespace Moongate.Server.Bootstrap;

public static class MoongateBootstrap
{
    public static WebApplication Build(MoongateBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var startTime = Stopwatch.GetTimestamp();
        var context = CreateContext(options);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        var app = builder.Build();
        ConfigurePipeline(app, startTime);

        return app;
    }

    public static async Task RunAsync(MoongateBootstrapOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var startTime = Stopwatch.GetTimestamp();
        var context = CreateContext(options);
        using var pidFileGuard = PidFileGuard.Acquire(context.Directories);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        await using var app = builder.Build();
        ConfigurePipeline(app, startTime);

        await app.RunAsync(cancellationToken);
    }

    internal static void ConfigureContainer(IContainer container, MoongateBootstrapContext context)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(context);

        container.RegisterBootstrapContext(context);
        container.AddObservability();
        container.AddDomainServices();                 // registers persisted entities before persistence
        container.AddDataPersistence(context);         // persistence, then bundled assets + UO/world data
        container.AddNetworkAndScripting(context);     // plugins must run before config
        container.LoadConfigurationAndLogger(context); // config last, then the real logger
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

        builder.AddMoongateAspNetServices(context);

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

    private static void ConfigurePipeline(WebApplication app, long startTime)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseServerReadyHook(startTime);
        app.MapMoongateHttpPipeline();
    }
}
