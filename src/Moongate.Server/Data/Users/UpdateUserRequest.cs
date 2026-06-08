namespace Moongate.Server.Data.Users;

/// <summary>Payload to update a user's email and level.</summary>
public sealed record UpdateUserRequest
{
    public string Email { get; init; } = "";
    public string Level { get; init; } = "Player";
}
