using Moongate.Core.Types;
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
}
