using System.Security.Claims;
using System.Security.Cryptography;
using Moongate.Core.Types;
using Moongate.Server.Data.Auth;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Auth;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Extensions.Endpoints;

public static class AuthEndpointExtensions
{
    private const int ActivationIdByteCount = 32;

    public static IEndpointRouteBuilder MapMoongateAuth(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
                             .WithTags("Auth");

        group.MapPost(
                 "/register",
                 (
                     AuthRegisterRequest request,
                     IUserService users,
                     ServerConfig config,
                     CancellationToken cancellationToken
                 ) => HandleRegisterAsync(request, users, config, cancellationToken)
             )
             .AllowAnonymous()
             .WithName("RegisterUser")
             .WithSummary("Registers a public player account when registration is enabled.")
             .Produces<AuthUserResponse>(StatusCodes.Status201Created)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces(StatusCodes.Status409Conflict);

        group.MapPost(
                 "/activate",
                 (
                     AuthActivationRequest request,
                     IUserService users,
                     CancellationToken cancellationToken
                 ) => HandleActivateAsync(request, users, cancellationToken)
             )
             .AllowAnonymous()
             .WithName("ActivateUser")
             .WithSummary("Activates a pending user account with an activation id.")
             .Produces<AuthUserResponse>()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        group.MapPost(
                 "/login",
                 (
                     AuthLoginRequest request,
                     IAuthTokenService auth,
                     CancellationToken cancellationToken
                 ) => HandleLoginAsync(request, auth, cancellationToken)
             )
             .AllowAnonymous()
             .WithName("Login")
             .WithSummary("Authenticates a user and returns an access token and refresh token.")
             .Produces<AuthTokenResponse>()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost(
                 "/refresh",
                 (
                     AuthRefreshRequest request,
                     IAuthTokenService auth,
                     CancellationToken cancellationToken
                 ) => HandleRefreshAsync(request, auth, cancellationToken)
             )
             .AllowAnonymous()
             .WithName("RefreshToken")
             .WithSummary("Refreshes an active web auth session.")
             .Produces<AuthTokenResponse>()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost(
                 "/logout",
                 (
                     AuthLogoutRequest request,
                     IAuthTokenService auth,
                     CancellationToken cancellationToken
                 ) => HandleLogoutAsync(request, auth, cancellationToken)
             )
             .AllowAnonymous()
             .WithName("Logout")
             .WithSummary("Revokes a refresh token.")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", (ClaimsPrincipal user) => HandleMe(user))
             .RequireAuthorization()
             .WithName("GetCurrentUser")
             .WithSummary("Returns the current authenticated web user.")
             .Produces<AuthUserResponse>()
             .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    internal static async Task<IResult> HandleActivateAsync(
        AuthActivationRequest request,
        IUserService users,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(users);

        if (string.IsNullOrWhiteSpace(request.ActivationId))
        {
            return TypedResults.BadRequest("activation_id is required");
        }

        var user = await users.ActivateAsync(request.ActivationId, cancellationToken);

        return user is null ? TypedResults.NotFound() : TypedResults.Ok(ToAuthUserResponse(user));
    }

    internal static async Task<IResult> HandleLoginAsync(
        AuthLoginRequest request,
        IAuthTokenService auth,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(auth);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.BadRequest("username and password are required");
        }

        var response = await auth.LoginAsync(request.Username, request.Password, cancellationToken);

        return response is null ? TypedResults.Unauthorized() : TypedResults.Ok(response);
    }

    internal static async Task<IResult> HandleRegisterAsync(
        AuthRegisterRequest request,
        IUserService users,
        ServerConfig config,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsRegistrationAllowed)
        {
            return TypedResults.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.BadRequest("username, email and password are required");
        }

        try
        {
            var user = await users.CreateAsync(
                           request.Username,
                           request.Email,
                           request.Password,
                           UserLevelType.Player,
                           false,
                           GenerateActivationId(),
                           cancellationToken
                       );

            return TypedResults.Created((string?)null, ToAuthUserResponse(user));
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }

    internal static async Task<IResult> HandleLogoutAsync(
        AuthLogoutRequest request,
        IAuthTokenService auth,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(auth);

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return TypedResults.BadRequest("refresh_token is required");
        }

        var revoked = await auth.LogoutAsync(request.RefreshToken, cancellationToken);

        return revoked ? TypedResults.NoContent() : TypedResults.Unauthorized();
    }

    internal static IResult HandleMe(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var username = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity?.Name ?? "";
        var level = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var activeText = user.FindFirst("is_active")?.Value;
        var isActive = !bool.TryParse(activeText, out var parsed) || parsed;

        return TypedResults.Ok(new AuthUserResponse(id, username, level, isActive));
    }

    internal static async Task<IResult> HandleRefreshAsync(
        AuthRefreshRequest request,
        IAuthTokenService auth,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(auth);

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return TypedResults.BadRequest("refresh_token is required");
        }

        var response = await auth.RefreshAsync(request.RefreshToken, cancellationToken);

        return response is null ? TypedResults.Unauthorized() : TypedResults.Ok(response);
    }

    private static string GenerateActivationId()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(ActivationIdByteCount)).ToLowerInvariant();

    private static AuthUserResponse ToAuthUserResponse(UserEntity user)
        => new(user.Id.ToString(), user.Username, user.Level.ToString(), user.IsActive);
}
