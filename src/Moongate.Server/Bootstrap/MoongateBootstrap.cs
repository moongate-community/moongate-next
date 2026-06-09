using System.Diagnostics;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
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
using Moongate.Server.Extensions.Auth;
using Moongate.Server.Extensions.Commands;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Endpoints;
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
using Moongate.Server.Services.Auth;
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
        container.AddDomainServices();              // registers persisted entities before persistence
        container.AddDataPersistence(context);      // persistence, then bundled assets + UO/world data
        container.AddNetworkAndScripting(context);  // plugins must run before config
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

        // ASP.NET Core services (OpenAPI, Kestrel, routing, ...) register through IServiceCollection.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(
            options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new()
                    {
                        Title = "Moongate API",
                        Version = "v1"
                    }
                );
            }
        );
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigurator>();

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

    private static void ConfigurePipeline(WebApplication app, long startTime)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Publish a tick event the moment the host is up; the handler logs the thread it runs on.
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(
            () =>
            {
                var elapsed = Stopwatch.GetElapsedTime(startTime);

                Log.Information(
                    "Moongate server ready in {Elapsed} ({ElapsedMilliseconds:F0} ms)",
                    elapsed,
                    elapsed.TotalMilliseconds
                );

                var bus = app.Services.GetRequiredService<IEventBusService>();
                bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));
            }
        );

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapMoongateApiDocs();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapMoongateAuth();
        app.MapMoongateAdminUsers();
        app.MapMoongateVersion();
        app.MapMoongateMetrics();
        app.MapMoongateMapImages();
        app.MapMoongateItemImages();
        app.MapFallbackToFile("index.html");
    }
}
