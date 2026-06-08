namespace Moongate.Server.Data.Auth;

/// <summary>Represents a web auth token pair.</summary>
public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUserResponse User
);
