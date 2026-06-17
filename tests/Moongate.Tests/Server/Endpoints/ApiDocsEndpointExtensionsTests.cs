using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Moongate.Server.Extensions.Endpoints;

namespace Moongate.Tests.Server.Endpoints;

public sealed class ApiDocsEndpointExtensionsTests
{
    [Fact]
    public async Task SwaggerDocument_WithDryIocServiceProvider_ReturnsDocument()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                EnvironmentName = Environments.Development,
                Args = []
            }
        );
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Moongate API",
                        Version = "v1"
                    }
                );
            }
        );

        await using var app = builder.Build();
        app.MapGet("/api/test", () => "ok")
            .WithName("Test");
        app.UseSwagger();
        app.MapMoongateApiDocs();

        await app.StartAsync();

        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient();

            var response = await client.GetAsync(new Uri(new Uri(address), "/swagger/v1/swagger.json"));
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.StatusCode}: {body}");
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"openapi\"", body, StringComparison.Ordinal);
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
