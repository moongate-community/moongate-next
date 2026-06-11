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

    internal static IResult HandleList(IPluginCatalogService plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        return TypedResults.Ok(plugins.GetLoadedPlugins());
    }
}
