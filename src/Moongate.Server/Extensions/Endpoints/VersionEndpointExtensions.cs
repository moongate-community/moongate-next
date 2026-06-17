using Moongate.Core.Utils;
using Moongate.Server.Data;

namespace Moongate.Server.Extensions.Endpoints;

public static class VersionEndpointExtensions
{
    public static IEndpointConventionBuilder MapMoongateVersion(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/version"
    )
    {
        var assembly = typeof(VersionEndpointExtensions).Assembly;
        var info = new ServerVersionInfo(
            VersionUtils.GetVersion(assembly),
            VersionUtils.GetMetadata(assembly, "Codename")
        );

        return endpoints.MapGet(pattern, () => Results.Json(info))
            .WithName("GetVersion");
    }
}
