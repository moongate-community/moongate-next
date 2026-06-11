namespace Moongate.Server.Data.Auth;

/// <summary>Payload for registering a public player account.</summary>
public sealed record AuthRegisterRequest
{
    /// <summary>Account username.</summary>
    public string Username { get; init; } = "";

    /// <summary>Account email address.</summary>
    public string Email { get; init; } = "";

    /// <summary>Account password.</summary>
    public string Password { get; init; } = "";
}
