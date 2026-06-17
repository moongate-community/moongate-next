using Scalar.AspNetCore;

namespace Moongate.Server.Extensions.Endpoints;

public static class ApiDocsEndpointExtensions
{
    private const string OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";

    public static IEndpointConventionBuilder MapMoongateApiDocs(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/docs"
    )
    {
        return endpoints.MapScalarApiReference(
            pattern,
            options =>
            {
                options.Title = "Moongate API";
                options.Theme = ScalarTheme.DeepSpace;
                options.OpenApiRoutePattern = OpenApiRoutePattern;
            }
        );
    }
}
