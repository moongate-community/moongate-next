using System.Globalization;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Server.Extensions.Endpoints;

public static class AdminHueEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateAdminHues(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/hues")
            .WithTags("Admin Hues")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                "/",
                (IHueStore hues, int? page, int? pageSize, string? search) =>
                    HandleListHues(hues, page, pageSize, search)
            )
            .WithName("ListAdminHues")
            .WithSummary("Returns a paged list of UO hues for the hue picker.")
            .Produces<PagedResult<HueSummary>>();

        group.MapGet(
                "/{hue:int}",
                (IHueStore hues, int hue) => HandleGetHue(hues, hue)
            )
            .WithName("GetAdminHue")
            .WithSummary("Returns a UO hue palette descriptor.");

        return endpoints;
    }

    internal static IResult HandleGetHue(IHueStore hues, int hue)
    {
        ArgumentNullException.ThrowIfNull(hues);

        var summary = HueSummary.FromValue(hue, hues);

        return summary.IsKnown ? TypedResults.Ok(summary) : TypedResults.NotFound();
    }

    internal static IResult HandleListHues(IHueStore hues, int? page, int? pageSize, string? search)
    {
        ArgumentNullException.ThrowIfNull(hues);

        var pageNumber = page is > 0 ? page.Value : 1;
        var size = pageSize is > 0 and <= 200 ? pageSize.Value : 60;

        var values = Enumerable.Range(1, hues.Count);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            values = values.Where(value => MatchesHue(value, term, hues));
        }

        var ordered = values.ToArray();

        var items = ordered
            .Skip((pageNumber - 1) * size)
            .Take(size)
            .Select(value => HueSummary.FromValue(value, hues))
            .ToArray();

        return TypedResults.Ok(new PagedResult<HueSummary>(items, pageNumber, size, ordered.Length));
    }

    private static bool MatchesHue(int value, string term, IHueStore hues)
    {
        if (term.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(term[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) &&
                   hex == value;
        }

        if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec))
        {
            return dec == value;
        }

        var hue = hues.GetHue(value - 1);

        return hue is not null && hue.Name.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
