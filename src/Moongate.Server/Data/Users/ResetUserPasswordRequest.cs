namespace Moongate.Server.Data.Users;

/// <summary>Payload to reset a user's password.</summary>
public sealed record ResetUserPasswordRequest
{
    public string Password { get; init; } = "";
}
