using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Moongate.Core.Ids;
using Moongate.Core.Utils;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Data.Auth;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Auth;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Auth;

public sealed class AuthTokenService : IAuthTokenService
{
    private const int RefreshTokenByteCount = 32;

    private readonly ILogger _logger = Log.ForContext<AuthTokenService>();
    private readonly Func<IUserService> _usersFactory;
    private readonly Func<IAutoDataAccess<AuthRefreshTokenEntity, Serial>> _refreshTokensFactory;
    private readonly WebConfig _webConfig;
    private readonly Func<DateTimeOffset> _now;

    private IUserService? _users;
    private IAutoDataAccess<AuthRefreshTokenEntity, Serial>? _refreshTokens;

    private IUserService Users => _users ??= _usersFactory();

    private IAutoDataAccess<AuthRefreshTokenEntity, Serial> RefreshTokens
        => _refreshTokens ??= _refreshTokensFactory();

    public AuthTokenService(
        Func<IUserService> usersFactory,
        Func<IAutoDataAccess<AuthRefreshTokenEntity, Serial>> refreshTokensFactory,
        WebConfig webConfig
    )
        : this(usersFactory, refreshTokensFactory, webConfig, static () => DateTimeOffset.UtcNow)
    {
    }

    internal AuthTokenService(
        IUserService users,
        IAutoDataAccess<AuthRefreshTokenEntity, Serial> refreshTokens,
        WebConfig webConfig,
        Func<DateTimeOffset> now
    )
        : this(() => users, () => refreshTokens, webConfig, now)
    {
    }

    internal AuthTokenService(
        Func<IUserService> usersFactory,
        Func<IAutoDataAccess<AuthRefreshTokenEntity, Serial>> refreshTokensFactory,
        WebConfig webConfig,
        Func<DateTimeOffset> now
    )
    {
        _usersFactory = usersFactory;
        _refreshTokensFactory = refreshTokensFactory;
        _webConfig = webConfig;
        _now = now;

        if (_webConfig.Jwt.IsUsingDevelopmentSigningKey)
        {
            _logger.Warning("Using development JWT signing key. Configure web.jwt.signing_key for production.");
        }
    }

    public async ValueTask<AuthTokenResponse?> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await Users.GetByUsernameAsync(username.Trim(), cancellationToken);

        if (user is null || !user.IsActive || !HashUtils.VerifyPassword(password, user.Password))
        {
            return null;
        }

        return await IssueTokenPairAsync(user, cancellationToken);
    }

    public async ValueTask<AuthTokenResponse?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var now = _now();
        var entity = FindRefreshToken(refreshToken);

        if (entity is null || !entity.IsActive(now))
        {
            return null;
        }

        var user = await Users.GetByIdAsync(entity.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (_webConfig.Jwt.RotateRefreshTokens)
        {
            entity.RevokedAt = now;
            await RefreshTokens.UpsertAsync(entity, cancellationToken);

            return await IssueTokenPairAsync(user, cancellationToken);
        }

        var accessTokenExpiresAt = now.AddMinutes(_webConfig.Jwt.AccessTokenMinutes);

        return new(
            CreateAccessToken(user, now, accessTokenExpiresAt),
            refreshToken,
            accessTokenExpiresAt,
            entity.ExpiresAt,
            CreateUserResponse(user)
        );
    }

    public async ValueTask<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var now = _now();
        var entity = FindRefreshToken(refreshToken);

        if (entity is null || !entity.IsActive(now))
        {
            return false;
        }

        entity.RevokedAt = now;
        await RefreshTokens.UpsertAsync(entity, cancellationToken);

        return true;
    }

    private static AuthUserResponse CreateUserResponse(UserEntity user)
        => new(user.Id.ToString(), user.Username, user.Level.ToString(), user.IsActive);

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenByteCount);

        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return "";
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToBase64String(bytes);
    }

    private string CreateAccessToken(UserEntity user, DateTimeOffset now, DateTimeOffset expiresAt)
    {
        var jwt = _webConfig.Jwt;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var userId = user.Id.ToString();
        var level = user.Level.ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, level),
            new Claim("user_id", userId),
            new Claim("role", level),
            new Claim("level", level),
            new Claim("is_active", user.IsActive.ToString())
        };
        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            now.UtcDateTime,
            expiresAt.UtcDateTime,
            credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AuthRefreshTokenEntity? FindRefreshToken(string refreshToken)
    {
        var hash = HashRefreshToken(refreshToken);

        if (hash.Length == 0)
        {
            return null;
        }

        return RefreshTokens.Query().FirstOrDefault(
            token => string.Equals(token.TokenHash, hash, StringComparison.Ordinal)
        );
    }

    private async ValueTask<AuthTokenResponse> IssueTokenPairAsync(
        UserEntity user,
        CancellationToken cancellationToken
    )
    {
        var now = _now();
        var accessTokenExpiresAt = now.AddMinutes(_webConfig.Jwt.AccessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(_webConfig.Jwt.RefreshTokenDays);
        var refreshToken = GenerateRefreshToken();
        var entity = new AuthRefreshTokenEntity(
            await RefreshTokens.NextIdAsync(cancellationToken),
            user.Id,
            HashRefreshToken(refreshToken),
            now,
            refreshTokenExpiresAt,
            null
        );

        await RefreshTokens.UpsertAsync(entity, cancellationToken);

        return new(
            CreateAccessToken(user, now, accessTokenExpiresAt),
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            CreateUserResponse(user)
        );
    }
}
