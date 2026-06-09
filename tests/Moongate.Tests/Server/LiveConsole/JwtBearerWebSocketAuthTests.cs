using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moongate.Server.Services.Auth;

namespace Moongate.Tests.Server.LiveConsole;

public class JwtBearerWebSocketAuthTests
{
    private static IQueryCollection Query(params (string Key, string Value)[] pairs)
        => new QueryCollection(pairs.ToDictionary(p => p.Key, p => new StringValues(p.Value)));

    [Fact]
    public void ResolveWebSocketToken_HubPathWithToken_ReturnsToken()
    {
        var token = JwtBearerOptionsConfigurator.ResolveWebSocketToken(
            Query(("access_token", "abc.def.ghi")),
            new PathString("/hubs/console")
        );

        Assert.Equal("abc.def.ghi", token);
    }

    [Fact]
    public void ResolveWebSocketToken_OtherPath_ReturnsNull()
    {
        var token = JwtBearerOptionsConfigurator.ResolveWebSocketToken(
            Query(("access_token", "abc.def.ghi")),
            new PathString("/api/auth/me")
        );

        Assert.Null(token);
    }

    [Fact]
    public void ResolveWebSocketToken_NoToken_ReturnsNull()
    {
        var token = JwtBearerOptionsConfigurator.ResolveWebSocketToken(
            Query(),
            new PathString("/hubs/console")
        );

        Assert.Null(token);
    }

    [Fact]
    public void ResolveWebSocketToken_PrefixAdjacentPath_ReturnsNull()
    {
        var token = JwtBearerOptionsConfigurator.ResolveWebSocketToken(
            Query(("access_token", "abc.def.ghi")),
            new PathString("/hubs/consolexyz")
        );

        Assert.Null(token);
    }
}
