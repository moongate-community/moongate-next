using Moongate.Core.Ids;

namespace Moongate.Server.Data.Auth;

public sealed class AuthRefreshTokenEntity
{
    public AuthRefreshTokenEntity(
        Serial id,
        Serial userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt
    )
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }

    public Serial Id { get; set; }
    public Serial UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ExpiresAt > now;
    }
}
