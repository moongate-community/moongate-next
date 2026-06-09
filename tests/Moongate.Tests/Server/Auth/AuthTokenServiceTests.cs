using System.IdentityModel.Tokens.Jwt;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Data.Auth;
using Moongate.Server.Services.Auth;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Tests.Server.Auth;

public sealed class AuthTokenServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 12, 0, 0, TimeSpan.Zero);

    private sealed class FakeUserService : IUserService
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _nextId = 1;

        public UserEntity Add(string username, string password, UserLevelType level, bool isActive)
        {
            var user = new UserEntity(
                new(_nextId++),
                username,
                $"{username}@test.local",
                HashUtils.HashPassword(password),
                level,
                isActive
            );
            _users[user.Id] = user;

            return user;
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Count);

        public ValueTask<UserEntity> CreateAsync(
            string username,
            string email,
            string password,
            UserLevelType level = UserLevelType.Player,
            bool isActive = true,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(Add(username, password, level, isActive));

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Remove(id));

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.GetValueOrDefault(id));

        public ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(
                _users.Values.FirstOrDefault(
                    user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)
                )
            );

        public async ValueTask<UserEntity?> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await GetByUsernameAsync(username, cancellationToken);

            return user is not null && user.IsActive && HashUtils.VerifyPassword(password, user.Password) ? user : null;
        }

        public ValueTask<PagedResult<UserEntity>> ListAsync(
            PageRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var all = _users.Values.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase).ToList();
            var items = all.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return ValueTask.FromResult(new PagedResult<UserEntity>(items, request.Page, request.PageSize, all.Count));
        }

        public ValueTask<bool> ResetPasswordAsync(
            Serial id,
            string newPassword,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(_users.ContainsKey(id));

        public ValueTask<UserEntity?> SetActiveAsync(Serial id, bool isActive, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.GetValueOrDefault(id));

        public ValueTask<UserEntity?> UpdateAsync(
            Serial id,
            string email,
            UserLevelType level,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(_users.GetValueOrDefault(id));
    }

    private sealed class FakeRefreshTokenAccess : IAutoDataAccess<AuthRefreshTokenEntity, Serial>
    {
        private readonly Dictionary<Serial, AuthRefreshTokenEntity> _tokens = [];
        private uint _nextId = 1;

        public IReadOnlyCollection<AuthRefreshTokenEntity> Tokens => _tokens.Values.Select(Clone).ToArray();

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_tokens.Count);

        public ValueTask<IReadOnlyCollection<AuthRefreshTokenEntity>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult<IReadOnlyCollection<AuthRefreshTokenEntity>>(Tokens);

        public ValueTask<AuthRefreshTokenEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_tokens.TryGetValue(id, out var token) ? Clone(token) : null);

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new Serial(_nextId++));

        public IQueryable<AuthRefreshTokenEntity> Query()
            => _tokens.Values.Select(Clone).AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_tokens.Remove(id));

        public ValueTask UpsertAsync(AuthRefreshTokenEntity entity, CancellationToken cancellationToken = default)
        {
            _tokens[entity.Id] = Clone(entity);

            return ValueTask.CompletedTask;
        }

        private static AuthRefreshTokenEntity Clone(AuthRefreshTokenEntity token)
            => new(
                token.Id,
                token.UserId,
                token.TokenHash,
                token.CreatedAt,
                token.ExpiresAt,
                token.RevokedAt
            );
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNullAndDoesNotStoreRefreshToken()
    {
        var users = new FakeUserService();
        users.Add("admin", "secret", UserLevelType.Administrator, true);
        var refreshTokens = new FakeRefreshTokenAccess();
        var service = CreateService(users, refreshTokens);

        var response = await service.LoginAsync("admin", "wrong", CancellationToken.None);

        Assert.Null(response);
        Assert.Empty(refreshTokens.Tokens);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsJwtAndStoresRefreshToken()
    {
        var users = new FakeUserService();
        var user = users.Add("admin", "secret", UserLevelType.Administrator, true);
        var refreshTokens = new FakeRefreshTokenAccess();
        var service = CreateService(users, refreshTokens);

        var response = await service.LoginAsync("admin", "secret", CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotEqual("", response!.AccessToken);
        Assert.NotEqual("", response.RefreshToken);
        Assert.Equal(FixedNow.AddMinutes(15), response.AccessTokenExpiresAt);
        Assert.Equal(FixedNow.AddDays(14), response.RefreshTokenExpiresAt);
        Assert.Equal(user.Id.ToString(), response.User.Id);
        Assert.Equal("admin", response.User.Username);
        Assert.Equal(UserLevelType.Administrator.ToString(), response.User.Level);

        var token = Assert.Single(refreshTokens.Tokens);
        Assert.Equal(user.Id, token.UserId);
        Assert.NotEqual(response.RefreshToken, token.TokenHash);
        Assert.Null(token.RevokedAt);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Contains(jwt.Claims, claim => claim.Type == "role" && claim.Value == UserLevelType.Administrator.ToString());
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesRefreshToken()
    {
        var users = new FakeUserService();
        users.Add("admin", "secret", UserLevelType.Administrator, true);
        var refreshTokens = new FakeRefreshTokenAccess();
        var service = CreateService(users, refreshTokens);
        var login = await service.LoginAsync("admin", "secret", CancellationToken.None);

        var loggedOut = await service.LogoutAsync(login!.RefreshToken, CancellationToken.None);
        var loggedOutAgain = await service.LogoutAsync(login.RefreshToken, CancellationToken.None);

        Assert.True(loggedOut);
        Assert.False(loggedOutAgain);
        Assert.NotNull(Assert.Single(refreshTokens.Tokens).RevokedAt);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesRefreshTokenAndRevokesOldToken()
    {
        var users = new FakeUserService();
        users.Add("admin", "secret", UserLevelType.Administrator, true);
        var refreshTokens = new FakeRefreshTokenAccess();
        var service = CreateService(users, refreshTokens);
        var login = await service.LoginAsync("admin", "secret", CancellationToken.None);

        var refreshed = await service.RefreshAsync(login!.RefreshToken, CancellationToken.None);
        var reused = await service.RefreshAsync(login.RefreshToken, CancellationToken.None);

        Assert.NotNull(refreshed);
        Assert.NotEqual(login.RefreshToken, refreshed!.RefreshToken);
        Assert.Null(reused);
        Assert.Equal(2, refreshTokens.Tokens.Count);
        Assert.NotNull(refreshTokens.Tokens.Single(token => token.Id == new Serial(1)).RevokedAt);
        Assert.Null(refreshTokens.Tokens.Single(token => token.Id == new Serial(2)).RevokedAt);
    }

    private static AuthTokenService CreateService(FakeUserService users, FakeRefreshTokenAccess refreshTokens)
        => new(users, refreshTokens, new(), () => FixedNow);
}
