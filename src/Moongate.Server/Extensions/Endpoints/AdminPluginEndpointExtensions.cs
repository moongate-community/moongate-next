using Moongate.Core.Types;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.Server.Extensions.Endpoints;

public static class AdminPluginEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateAdminPlugins(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/plugins")
                             .WithTags("Admin Plugins")
                             .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                 "/",
                 (IPluginCatalogService plugins) => HandleList(plugins)
             )
             .WithName("ListPlugins")
             .WithSummary("Returns the loaded plugin catalog.")
             .Produces<IReadOnlyList<PluginCatalogEntry>>();

        group.MapGet(
                 "/{id}",
                 (IPluginCatalogService plugins, string id) => HandleGet(plugins, id)
             )
             .WithName("GetPlugin")
             .WithSummary("Returns metadata for a loaded plugin.")
             .Produces<PluginCatalogEntry>()
             .Produces(StatusCodes.Status404NotFound);

        group.MapGet(
                 "/{id}/config",
                 (IPluginCatalogService plugins, string id, CancellationToken cancellationToken)
                     => HandleGetConfigAsync(plugins, id, cancellationToken)
             )
             .WithName("GetPluginConfig")
             .WithSummary("Returns a sanitized view of a loaded plugin's runtime config.")
             .Produces<PluginConfigView>()
             .Produces(StatusCodes.Status404NotFound);

        group.MapGet(
                 "/{id}/config/form",
                 (IPluginCatalogService plugins, string id, CancellationToken cancellationToken)
                     => HandleGetConfigFormAsync(plugins, id, cancellationToken)
             )
             .WithName("GetPluginConfigForm")
             .WithSummary("Returns a simple editable config form for a loaded plugin.")
             .Produces<PluginConfigForm>()
             .Produces(StatusCodes.Status404NotFound);

        group.MapPut(
                 "/{id}/config",
                 (IPluginCatalogService plugins, string id, PluginConfigSaveRequest request, CancellationToken cancellationToken)
                     => HandleSaveConfigAsync(plugins, id, request, cancellationToken)
             )
             .WithName("SavePluginConfig")
             .WithSummary("Saves editable config values for a loaded plugin.")
             .Produces<PluginConfigSaveResult>()
             .Produces(StatusCodes.Status404NotFound);

        group.MapPost(
                 "/{id}/test",
                 (IPluginCatalogService plugins, string id, CancellationToken cancellationToken)
                     => HandleTestAsync(plugins, id, cancellationToken)
             )
             .WithName("TestPluginConfig")
             .WithSummary("Runs a plugin-specific configuration test.")
             .Produces<PluginTestResult>()
             .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    internal static IResult HandleGet(IPluginCatalogService plugins, string id)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var plugin = plugins
                     .GetLoadedPlugins()
                     .FirstOrDefault(entry => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));

        return plugin is null ? TypedResults.NotFound() : TypedResults.Ok(plugin);
    }

    internal static async Task<IResult> HandleGetConfigAsync(
        IPluginCatalogService plugins,
        string id,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var config = await plugins.GetConfigAsync(id, cancellationToken);

        return config is null ? TypedResults.NotFound() : TypedResults.Ok(config);
    }

    internal static async Task<IResult> HandleGetConfigFormAsync(
        IPluginCatalogService plugins,
        string id,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var form = await plugins.GetConfigFormAsync(id, cancellationToken);

        return form is null ? TypedResults.NotFound() : TypedResults.Ok(form);
    }

    internal static IResult HandleList(IPluginCatalogService plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        return TypedResults.Ok(plugins.GetLoadedPlugins());
    }

    internal static async Task<IResult> HandleSaveConfigAsync(
        IPluginCatalogService plugins,
        string id,
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(plugins);
        ArgumentNullException.ThrowIfNull(request);

        var result = await plugins.SaveConfigAsync(id, request, cancellationToken);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    internal static async Task<IResult> HandleTestAsync(
        IPluginCatalogService plugins,
        string id,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var result = await plugins.TestAsync(id, cancellationToken);

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}
