using System.Globalization;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;
using Moongate.Server.Data.Templates;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Extensions.Endpoints;

public static class ItemTemplateEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateItemTemplates(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/item-templates")
            .WithTags("Admin Item Templates")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                "/",
                (
                    IItemTemplateService templates,
                    IHueStore hues,
                    HttpRequest request,
                    int? page,
                    int? pageSize,
                    string? search,
                    string? tag,
                    string? rarity,
                    string? layer
                ) => HandleList(
                    templates,
                    hues,
                    page,
                    pageSize,
                    search,
                    tag,
                    rarity,
                    layer,
                    request.Query["abstract"].FirstOrDefault()
                )
            )
            .WithName("ListItemTemplates")
            .WithSummary("Returns a paginated, searchable list of item templates.");

        group.MapGet(
                "/{id}",
                (IItemTemplateService templates, IHueStore hues, string id) => HandleDetail(templates, hues, id)
            )
            .WithName("GetItemTemplate")
            .WithSummary("Returns a full read-only item template definition.");

        group.MapPost(
                "/",
                (
                    IItemTemplateAuthoringService authoring,
                    ItemTemplateEditRequest request,
                    CancellationToken cancellationToken
                ) => HandleCreateAsync(authoring, request, cancellationToken)
            )
            .WithName("CreateItemTemplate")
            .WithSummary("Creates an item template in the managed web YAML file.");

        group.MapPut(
                "/{id}",
                (
                    IItemTemplateAuthoringService authoring,
                    string id,
                    ItemTemplateEditRequest request,
                    CancellationToken cancellationToken
                ) => HandleUpdateAsync(authoring, id, request, cancellationToken)
            )
            .WithName("UpdateItemTemplate")
            .WithSummary("Updates an existing item template in its owning YAML file.");

        return endpoints;
    }

    internal static async Task<IResult> HandleCreateAsync(
        IItemTemplateAuthoringService authoring,
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(authoring);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var result = await authoring.CreateAsync(request, cancellationToken);

            return TypedResults.Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.BadRequest(exception.Message);
        }
    }

    internal static IResult HandleDetail(
        IItemTemplateService templates,
        IHueStore hues,
        string id
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(hues);

        return templates.TryGet(id, out var template)
            ? TypedResults.Ok(ItemTemplateDetail.FromDefinition(template, hues))
            : TypedResults.NotFound();
    }

    internal static IResult HandleList(
        IItemTemplateService templates,
        IHueStore hues,
        int? page,
        int? pageSize,
        string? search,
        string? tag,
        string? rarity,
        string? layer,
        string? abstractText
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(hues);

        if (!TryParseEnum<ItemRarity>(rarity, out var rarityFilter, out var rarityError))
        {
            return TypedResults.BadRequest(rarityError);
        }

        if (!TryParseEnum<ItemLayerType>(layer, out var layerFilter, out var layerError))
        {
            return TypedResults.BadRequest(layerError);
        }

        if (!TryParseOptionalBool(abstractText, out var abstractFilter))
        {
            return TypedResults.BadRequest("abstract must be true or false.");
        }

        var filters = new List<Func<ItemTemplateDefinition, bool>>();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            filters.Add(template => template.Tags.Any(templateTag => string.Equals(
                        templateTag,
                        tag.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            );
        }

        if (rarityFilter.HasValue)
        {
            filters.Add(template => template.Rarity == rarityFilter.Value);
        }

        if (layerFilter.HasValue)
        {
            filters.Add(template => template.Layer == layerFilter.Value);
        }

        if (abstractFilter.HasValue)
        {
            filters.Add(template => template.IsAbstract == abstractFilter.Value);
        }

        var request = PageRequest.Normalize(page, pageSize, search);
        var ordered = templates.GetAll().OrderBy(static template => template.Id, StringComparer.OrdinalIgnoreCase);
        var result = InMemoryListQuery.Apply(ordered, request, SearchFields, filters);

        return TypedResults.Ok(result.Select(template => ItemTemplateSummary.FromDefinition(template, hues)));
    }

    internal static async Task<IResult> HandleUpdateAsync(
        IItemTemplateAuthoringService authoring,
        string id,
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(authoring);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var result = await authoring.UpdateAsync(id, request, cancellationToken);

            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.BadRequest(exception.Message);
        }
    }

    private static IEnumerable<string?> SearchFields(ItemTemplateDefinition template)
    {
        yield return template.Id;
        yield return template.Name;
        yield return template.Comment;
        yield return template.ScriptId;
        yield return template.ItemId.ToString(CultureInfo.InvariantCulture);
        yield return ItemTemplateSummary.FormatItemId(template.ItemId);
        yield return $"0x{template.ItemId:X}";

        if (template.Value is not null)
        {
            yield return template.Value.Buy.ToString(CultureInfo.InvariantCulture);
            yield return template.Value.BaseSell.ToString(CultureInfo.InvariantCulture);
            yield return template.Value.EffectiveBuy(template.Rarity).ToString(CultureInfo.InvariantCulture);
            yield return template.Value.EffectiveSell(template.Rarity).ToString(CultureInfo.InvariantCulture);
        }

        foreach (var tag in template.Tags)
        {
            yield return tag;
        }
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum? parsed, out string error)
        where TEnum : struct
    {
        parsed = null;
        error = "";

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<TEnum>(value, true, out var result))
        {
            parsed = result;

            return true;
        }

        error = $"Unknown {typeof(TEnum).Name} '{value}'.";

        return false;
    }

    private static bool TryParseOptionalBool(string? value, out bool? parsed)
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!bool.TryParse(value, out var result))
        {
            return false;
        }

        parsed = result;

        return true;
    }
}
