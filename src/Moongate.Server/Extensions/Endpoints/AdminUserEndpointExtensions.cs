using System.Security.Claims;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Users;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Extensions.Endpoints;

public static class AdminUserEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateAdminUsers(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/users")
            .WithTags("Admin Users")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                "/",
                (IUserService users, int? page, int? pageSize, string? search, CancellationToken ct)
                    => HandleListAsync(users, page, pageSize, search, ct)
            )
            .WithName("ListUsers")
            .WithSummary("Returns a paginated, searchable list of users.");

        group.MapPost(
                "/",
                (IUserService users, CreateUserRequest request, CancellationToken ct)
                    => HandleCreateAsync(users, request, ct)
            )
            .WithName("CreateUser")
            .WithSummary("Creates a new user.");

        group.MapPut(
                "/{id}",
                (IUserService users, ClaimsPrincipal caller, string id, UpdateUserRequest request, CancellationToken ct)
                    => HandleUpdateAsync(users, caller, id, request, ct)
            )
            .WithName("UpdateUser")
            .WithSummary("Updates a user's email and level.");

        group.MapPost(
                "/{id}/active",
                (IUserService users, ClaimsPrincipal caller, string id, SetUserActiveRequest request, CancellationToken ct)
                    => HandleSetActiveAsync(users, caller, id, request, ct)
            )
            .WithName("SetUserActive")
            .WithSummary("Locks or unlocks a user.");

        group.MapPost(
                "/{id}/password",
                (IUserService users, string id, ResetUserPasswordRequest request, CancellationToken ct)
                    => HandleResetPasswordAsync(users, id, request, ct)
            )
            .WithName("ResetUserPassword")
            .WithSummary("Resets a user's password.");

        group.MapDelete(
                "/{id}",
                (IUserService users, ClaimsPrincipal caller, string id, CancellationToken ct)
                    => HandleDeleteAsync(users, caller, id, ct)
            )
            .WithName("DeleteUser")
            .WithSummary("Permanently deletes a user.");

        return endpoints;
    }

    internal static async Task<IResult> HandleCreateAsync(
        IUserService users,
        CreateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<UserLevelType>(request.Level, out var level))
        {
            return TypedResults.BadRequest($"Unknown level '{request.Level}'.");
        }

        try
        {
            var user = await users.CreateAsync(
                request.Username,
                request.Email,
                request.Password,
                level,
                request.IsActive,
                cancellationToken: cancellationToken
            );

            return TypedResults.Ok(UserSummary.FromEntity(user));
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

    internal static async Task<IResult> HandleDeleteAsync(
        IUserService users,
        ClaimsPrincipal caller,
        string id,
        CancellationToken cancellationToken
    )
    {
        if (IsSelf(caller, id))
        {
            return TypedResults.Forbid();
        }

        var deleted = await users.DeleteAsync(ParseId(id), cancellationToken);

        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    internal static async Task<IResult> HandleListAsync(
        IUserService users,
        int? page,
        int? pageSize,
        string? search,
        CancellationToken cancellationToken
    )
    {
        var request = PageRequest.Normalize(page, pageSize, search);
        var result = await users.ListAsync(request, cancellationToken);

        return TypedResults.Ok(result.Select(UserSummary.FromEntity));
    }

    internal static async Task<IResult> HandleResetPasswordAsync(
        IUserService users,
        string id,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return TypedResults.BadRequest("password is required");
        }

        var changed = await users.ResetPasswordAsync(ParseId(id), request.Password, cancellationToken);

        return changed ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    internal static async Task<IResult> HandleSetActiveAsync(
        IUserService users,
        ClaimsPrincipal caller,
        string id,
        SetUserActiveRequest request,
        CancellationToken cancellationToken
    )
    {
        if (IsSelf(caller, id) && !request.IsActive)
        {
            return TypedResults.Forbid();
        }

        var user = await users.SetActiveAsync(ParseId(id), request.IsActive, cancellationToken);

        return user is null ? TypedResults.NotFound() : TypedResults.Ok(UserSummary.FromEntity(user));
    }

    internal static async Task<IResult> HandleUpdateAsync(
        IUserService users,
        ClaimsPrincipal caller,
        string id,
        UpdateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<UserLevelType>(request.Level, out var level))
        {
            return TypedResults.BadRequest($"Unknown level '{request.Level}'.");
        }

        if (IsSelf(caller, id) && level != UserLevelType.Administrator)
        {
            return TypedResults.Forbid();
        }

        try
        {
            var user = await users.UpdateAsync(ParseId(id), request.Email, level, cancellationToken);

            return user is null ? TypedResults.NotFound() : TypedResults.Ok(UserSummary.FromEntity(user));
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

    private static bool IsSelf(ClaimsPrincipal caller, string id)
    {
        var current = caller.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return !string.IsNullOrEmpty(current) && string.Equals(current, id, StringComparison.OrdinalIgnoreCase);
    }

    private static Serial ParseId(string id)
    {
        return Serial.TryParse(id, null, out var serial) ? serial : Serial.MinusOne;
    }
}
