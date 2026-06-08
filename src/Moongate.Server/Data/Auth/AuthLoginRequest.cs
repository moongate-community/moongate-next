namespace Moongate.Server.Data.Auth;

/// <summary>Payload for starting a web auth session.</summary>
public sealed record AuthLoginRequest
{
    /// <summary>Account username.</summary>
    public string Username { get; init; } = "";

    /// <summary>Account password.</summary>
    public string Password { get; init; } = "";
}
