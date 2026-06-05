using Scalar.AspNetCore;

namespace Moongate.Server.Extensions.Endpoints;

public static class ApiDocsEndpointExtensions
{
    public static IEndpointConventionBuilder MapMoongateApiDocs(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/docs"
    )
    {
        endpoints.MapOpenApi();

        return endpoints.MapScalarApiReference(
            pattern,
            options =>
            {
                options.Title = "Moongate API";
                options.Theme = ScalarTheme.DeepSpace;
            }
        );
    }
}
