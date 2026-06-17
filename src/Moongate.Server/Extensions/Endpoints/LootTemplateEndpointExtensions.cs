using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;
using Moongate.Server.Data.Templates;
using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Server.Extensions.Endpoints;

public static class LootTemplateEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateLootTemplates(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/loot-templates")
            .WithTags("Admin Loot Templates")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                "/",
                (LootTableRegistryStore store, int? page, int? pageSize, string? search)
                    => HandleList(store.Registry, page, pageSize, search)
            )
            .WithName("ListLootTemplates")
            .WithSummary("Returns a paginated, searchable list of loot templates.");

        group.MapGet(
                "/{id}",
                (LootTableRegistryStore store, LootTemplateProjectionService projector, string id)
                    => HandleDetail(store.Registry, projector, id)
            )
            .WithName("GetLootTemplate")
            .WithSummary("Returns a full read-only loot template definition.");

        return endpoints;
    }

    internal static IResult HandleDetail(
        LootTableRegistry registry,
        LootTemplateProjectionService projector,
        string id
    )
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(projector);

        return registry.TryGet(id, out var table)
            ? TypedResults.Ok(projector.Project(table))
            : TypedResults.NotFound();
    }

    internal static IResult HandleList(LootTableRegistry registry, int? page, int? pageSize, string? search)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var request = PageRequest.Normalize(page, pageSize, search);
        var ordered = registry.GetAll().OrderBy(static table => table.Id, StringComparer.OrdinalIgnoreCase);
        var result = InMemoryListQuery.Apply(ordered, request, SearchFields, []);

        return TypedResults.Ok(result.Select(LootTemplateSummary.FromDefinition));
    }

    private static IEnumerable<string?> SearchFields(LootTableDefinition table)
    {
        yield return table.Id;
    }
}
