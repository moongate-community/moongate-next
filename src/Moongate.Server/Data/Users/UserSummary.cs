using Moongate.UO.Domain.Entities;

namespace Moongate.Server.Data.Users;

/// <summary>Public projection of a user for the admin API (no password hash).</summary>
public sealed record UserSummary(
    string Id,
    string Username,
    string Email,
    string Level,
    bool IsActive
)
{
    public static UserSummary FromEntity(UserEntity user)
        => new(user.Id.ToString(), user.Username, user.Email, user.Level.ToString(), user.IsActive);
}
