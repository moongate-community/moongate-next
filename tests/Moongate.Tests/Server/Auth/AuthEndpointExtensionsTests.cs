using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Server.Data.Auth;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Interfaces.Auth;
using Moongate.UO.Domain.Types;

namespace Moongate.Tests.Server.Auth;

public sealed class AuthEndpointExtensionsTests
{
    [Fact]
    public async Task HandleLoginAsync_MissingCredentials_ReturnsBadRequest()
    {
        var auth = new FakeAuthTokenService();
        var request = new AuthLoginRequest
        {
            Username = "",
            Password = ""
        };

        var result = await AuthEndpointExtensions.HandleLoginAsync(request, auth, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task HandleLoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var expected = CreateResponse();
        var auth = new FakeAuthTokenService
        {
            LoginResponse = expected
        };
        var request = new AuthLoginRequest
        {
            Username = "admin",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleLoginAsync(request, auth, CancellationToken.None);

        var ok = Assert.IsType<Ok<AuthTokenResponse>>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public void HandleMe_AuthenticatedPrincipal_ReturnsCurrentUser()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, "0x00000001"),
                    new(ClaimTypes.Name, "admin"),
                    new(ClaimTypes.Role, UserLevelType.Administrator.ToString())
                ],
                "jwt"
            )
        );

        var result = AuthEndpointExtensions.HandleMe(principal);

        var ok = Assert.IsType<Ok<AuthUserResponse>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal("0x00000001", ok.Value.Id);
        Assert.Equal("admin", ok.Value.Username);
        Assert.Equal(UserLevelType.Administrator.ToString(), ok.Value.Level);
    }

    [Fact]
    public async Task HandleRefreshAsync_InvalidRefreshToken_ReturnsUnauthorized()
    {
        var auth = new FakeAuthTokenService();
        var request = new AuthRefreshRequest
        {
            RefreshToken = "old"
        };

        var result = await AuthEndpointExtensions.HandleRefreshAsync(request, auth, CancellationToken.None);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    private static AuthTokenResponse CreateResponse()
        => new(
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddDays(14),
            new("0x00000001", "admin", UserLevelType.Administrator.ToString(), true)
        );

    private sealed class FakeAuthTokenService : IAuthTokenService
    {
        public AuthTokenResponse? LoginResponse { get; set; }
        public AuthTokenResponse? RefreshResponse { get; set; }
        public bool LogoutResponse { get; set; }

        public ValueTask<AuthTokenResponse?> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(LoginResponse);

        public ValueTask<AuthTokenResponse?> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(RefreshResponse);

        public ValueTask<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(LogoutResponse);
    }
}
