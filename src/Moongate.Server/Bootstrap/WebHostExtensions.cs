using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Internal;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Extensions.Metrics;
using Moongate.Server.Hubs;
using Moongate.Server.Services.Auth;
using Moongate.Server.Services.LiveConsole;
using Serilog;

namespace Moongate.Server.Bootstrap;

/// <summary>
/// Web-host concerns split out of <see cref="MoongateBootstrap" />: ASP.NET service registration,
/// the "server ready" lifecycle hook, and the HTTP request pipeline.
/// </summary>
internal static class WebHostExtensions
{
    /// <summary>Registers the ASP.NET Core services (OpenAPI, auth) and bridges the DI orchestrator.</summary>
    public static WebApplicationBuilder AddMoongateAspNetServices(
        this WebApplicationBuilder builder,
        MoongateBootstrapContext context
    )
    {
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

        // SignalR for the live admin console. Serialize enums as names so the client receives
        // "Log"/"CommandEcho"/"CommandOutput" instead of numbers.
        builder.Services
               .AddSignalR()
               .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Bridge the DryIoc-registered relay into the host's hosted-service collection (same pattern
        // as the orchestrator below).
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<LiveConsoleRelay>());

        // The generic host collects hosted services from IServiceCollection, so bridge the
        // DryIoc-registered orchestrator here; it resolves its descriptors from the unified provider.
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MoongateServiceOrchestrator>());

        Log.Information("Registered {PacketCount} UO packets", context.RegisteredPacketCount);

        return builder;
    }

    /// <summary>Wires the HTTP middleware and maps all Moongate endpoints.</summary>
    public static WebApplication MapMoongateHttpPipeline(this WebApplication app)
    {
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
        app.MapMoongateAdminPlugins();
        app.MapMoongateAdminUsers();
        app.MapMoongateItemTemplates();
        app.MapMoongateMobileTemplates();
        app.MapMoongateAdminHues();
        app.MapMoongateVersion();
        app.MapMoongateMetrics();
        app.MapMoongateMapImages();
        app.MapMoongateItemImages();
        app.MapMoongateBodyImages();
        app.MapMoongateMobileTemplateImages();
        app.MapHub<LiveConsoleHub>(LiveConsoleHub.Route);
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>Publishes <see cref="ServerStartedEvent" /> and logs readiness when the host starts.</summary>
    public static WebApplication UseServerReadyHook(this WebApplication app, long startTime)
    {
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

        return app;
    }
}
