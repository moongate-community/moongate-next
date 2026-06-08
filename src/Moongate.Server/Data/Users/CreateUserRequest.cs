namespace Moongate.Server.Data.Users;

/// <summary>Payload to create a user from the admin API.</summary>
public sealed record CreateUserRequest
{
    public string Username { get; init; } = "";
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
    public string Level { get; init; } = "Player";
    public bool IsActive { get; init; } = true;
}
