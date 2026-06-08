namespace Moongate.Server.Data.Auth;

/// <summary>Payload for ending a web auth session.</summary>
public sealed record AuthLogoutRequest
{
    /// <summary>Opaque refresh token to revoke.</summary>
    public string RefreshToken { get; init; } = "";
}
