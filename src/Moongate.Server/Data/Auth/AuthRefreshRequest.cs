namespace Moongate.Server.Data.Auth;

/// <summary>Payload for refreshing a web auth session.</summary>
public sealed record AuthRefreshRequest
{
    /// <summary>Opaque refresh token returned by login or refresh.</summary>
    public string RefreshToken { get; init; } = "";
}
