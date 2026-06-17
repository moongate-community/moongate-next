using System.Globalization;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;
using Moongate.Server.Data.Templates;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Server.Extensions.Endpoints;

public static class MobileTemplateEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateMobileTemplates(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/mobile-templates")
            .WithTags("Admin Mobile Templates")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                "/",
                (
                    IMobileTemplateService templates,
                    HttpRequest request,
                    int? page,
                    int? pageSize,
                    string? search,
                    string? tag,
                    string? notoriety
                ) => HandleList(
                    templates,
                    page,
                    pageSize,
                    search,
                    tag,
                    notoriety,
                    request.Query["abstract"].FirstOrDefault()
                )
            )
            .WithName("ListMobileTemplates")
            .WithSummary("Returns a paginated, searchable list of mobile templates.");

        group.MapGet(
                "/{id}",
                (IMobileTemplateService templates, string id) => HandleDetail(templates, id)
            )
            .WithName("GetMobileTemplate")
            .WithSummary("Returns a full read-only mobile template definition.");

        group.MapPost(
                "/",
                (
                    IMobileTemplateAuthoringService authoring,
                    MobileTemplateEditRequest request,
                    CancellationToken cancellationToken
                ) => HandleCreateAsync(authoring, request, cancellationToken)
            )
            .WithName("CreateMobileTemplate")
            .WithSummary("Creates a mobile template in the managed web YAML file.");

        group.MapPut(
                "/{id}",
                (
                    IMobileTemplateAuthoringService authoring,
                    string id,
                    MobileTemplateEditRequest request,
                    CancellationToken cancellationToken
                ) => HandleUpdateAsync(authoring, id, request, cancellationToken)
            )
            .WithName("UpdateMobileTemplate")
            .WithSummary("Updates an existing mobile template in its owning YAML file.");

        return endpoints;
    }

    internal static async Task<IResult> HandleCreateAsync(
        IMobileTemplateAuthoringService authoring,
        MobileTemplateEditRequest request,
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

    internal static IResult HandleDetail(IMobileTemplateService templates, string id)
    {
        ArgumentNullException.ThrowIfNull(templates);

        return templates.TryGet(id, out var template)
            ? TypedResults.Ok(MobileTemplateDetail.FromDefinition(template!))
            : TypedResults.NotFound();
    }

    internal static IResult HandleList(
        IMobileTemplateService templates,
        int? page,
        int? pageSize,
        string? search,
        string? tag,
        string? notoriety,
        string? abstractText
    )
    {
        ArgumentNullException.ThrowIfNull(templates);

        if (!TryParseEnum<NotorietyType>(notoriety, out var notorietyFilter, out var notorietyError))
        {
            return TypedResults.BadRequest(notorietyError);
        }

        if (!TryParseOptionalBool(abstractText, out var abstractFilter))
        {
            return TypedResults.BadRequest("abstract must be true or false.");
        }

        var filters = new List<Func<MobileTemplateDefinition, bool>>();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            filters.Add(template =>
                template.Tags.Any(templateTag => string.Equals(templateTag, tag.Trim(), StringComparison.OrdinalIgnoreCase)
                )
            );
        }

        if (notorietyFilter.HasValue)
        {
            filters.Add(template => template.Notoriety == notorietyFilter.Value);
        }

        if (abstractFilter.HasValue)
        {
            filters.Add(template => template.IsAbstract == abstractFilter.Value);
        }

        var request = PageRequest.Normalize(page, pageSize, search);
        var ordered = templates.GetAll().OrderBy(static template => template.Id, StringComparer.OrdinalIgnoreCase);
        var result = InMemoryListQuery.Apply(ordered, request, SearchFields, filters);

        return TypedResults.Ok(result.Select(MobileTemplateSummary.FromDefinition));
    }

    internal static async Task<IResult> HandleUpdateAsync(
        IMobileTemplateAuthoringService authoring,
        string id,
        MobileTemplateEditRequest request,
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

    private static IEnumerable<string?> SearchFields(MobileTemplateDefinition template)
    {
        yield return template.Id;
        yield return template.Name;
        yield return template.Title;
        yield return template.Brain;
        yield return template.FactionId;
        yield return template.Body.ToString(CultureInfo.InvariantCulture);
        yield return MobileTemplateSummary.FormatBody(template.Body);
        yield return $"0x{template.Body:X}";

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
