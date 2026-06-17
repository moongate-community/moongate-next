using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Auth;
using Moongate.Server.Data.Config;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Interfaces.Auth;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Tests.Server.Auth;

public sealed class AuthEndpointExtensionsTests
{
    [Fact]
    public async Task HandleActivateAsync_BlankActivationId_ReturnsBadRequest()
    {
        var users = new FakeUserService();
        var request = new AuthActivationRequest
        {
            ActivationId = ""
        };

        var result = await AuthEndpointExtensions.HandleActivateAsync(request, users, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task HandleActivateAsync_UnknownActivationId_ReturnsNotFound()
    {
        var users = new FakeUserService();
        var request = new AuthActivationRequest
        {
            ActivationId = "missing"
        };

        var result = await AuthEndpointExtensions.HandleActivateAsync(request, users, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleActivateAsync_ValidActivationId_ActivatesUser()
    {
        var users = new FakeUserService();
        var seeded = users.Seed("pending", false, "activation-token");
        var request = new AuthActivationRequest
        {
            ActivationId = " activation-token "
        };

        var result = await AuthEndpointExtensions.HandleActivateAsync(request, users, CancellationToken.None);

        var ok = Assert.IsType<Ok<AuthUserResponse>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal(seeded.Id.ToString(), ok.Value.Id);
        Assert.Equal("pending", ok.Value.Username);
        Assert.True(ok.Value.IsActive);
        Assert.True(seeded.IsActive);
        Assert.Null(seeded.ActivationId);
    }

    [Fact]
    public async Task HandleLoginAsync_MissingCredentials_ReturnsBadRequest()
    {
        var auth = new FakeAuthTokenService();
        var request = new AuthLoginRequest
        {
            Username = "",
            Password = ""
        };

        var result = await AuthEndpointExtensions.HandleLoginAsync(request, auth, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task HandleLoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var expected = CreateResponse();
        var auth = new FakeAuthTokenService
        {
            LoginResponse = expected
        };
        var request = new AuthLoginRequest
        {
            Username = "admin",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleLoginAsync(request, auth, CancellationToken.None);

        var ok = Assert.IsType<Ok<AuthTokenResponse>>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public void HandleMe_AuthenticatedPrincipal_ReturnsCurrentUser()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "0x00000001"),
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Role, UserLevelType.Administrator.ToString())
                ],
                "jwt"
            )
        );

        var result = AuthEndpointExtensions.HandleMe(principal);

        var ok = Assert.IsType<Ok<AuthUserResponse>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal("0x00000001", ok.Value.Id);
        Assert.Equal("admin", ok.Value.Username);
        Assert.Equal(UserLevelType.Administrator.ToString(), ok.Value.Level);
    }

    [Fact]
    public async Task HandleRefreshAsync_InvalidRefreshToken_ReturnsUnauthorized()
    {
        var auth = new FakeAuthTokenService();
        var request = new AuthRefreshRequest
        {
            RefreshToken = "old"
        };

        var result = await AuthEndpointExtensions.HandleRefreshAsync(request, auth, CancellationToken.None);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task HandleRegisterAsync_DisabledRegistration_ReturnsForbid()
    {
        var users = new FakeUserService();
        var request = new AuthRegisterRequest
        {
            Username = "pending",
            Email = "pending@realm.local",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleRegisterAsync(
            request,
            users,
            new ServerConfig(),
            CancellationToken.None
        );

        Assert.IsType<ForbidHttpResult>(result);
        Assert.Empty(users.Users);
    }

    [Fact]
    public async Task HandleRegisterAsync_DuplicateUser_ReturnsConflict()
    {
        var users = new FakeUserService
        {
            ThrowConflict = true
        };
        var request = new AuthRegisterRequest
        {
            Username = "pending",
            Email = "pending@realm.local",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleRegisterAsync(
            request,
            users,
            new ServerConfig { IsRegistrationAllowed = true },
            CancellationToken.None
        );

        Assert.IsType<Conflict<string>>(result);
    }

    [Fact]
    public async Task HandleRegisterAsync_EnabledRegistration_CreatesInactivePlayerWithActivationId()
    {
        var users = new FakeUserService();
        var request = new AuthRegisterRequest
        {
            Username = "pending",
            Email = "pending@realm.local",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleRegisterAsync(
            request,
            users,
            new ServerConfig { IsRegistrationAllowed = true },
            CancellationToken.None
        );

        var created = Assert.IsType<Created<AuthUserResponse>>(result);
        Assert.NotNull(created.Value);
        Assert.Equal("pending", created.Value.Username);
        Assert.Equal(UserLevelType.Player.ToString(), created.Value.Level);
        Assert.False(created.Value.IsActive);

        var user = Assert.Single(users.Users);
        Assert.Equal("pending", user.Username);
        Assert.Equal("pending@realm.local", user.Email);
        Assert.Equal(UserLevelType.Player, user.Level);
        Assert.False(user.IsActive);
        Assert.NotNull(user.ActivationId);
        Assert.Equal(64, user.ActivationId.Length);
    }

    [Fact]
    public async Task HandleRegisterAsync_MissingRequiredFields_ReturnsBadRequest()
    {
        var users = new FakeUserService();
        var request = new AuthRegisterRequest
        {
            Username = "",
            Email = "pending@realm.local",
            Password = "secret"
        };

        var result = await AuthEndpointExtensions.HandleRegisterAsync(
            request,
            users,
            new ServerConfig { IsRegistrationAllowed = true },
            CancellationToken.None
        );

        Assert.IsType<BadRequest<string>>(result);
        Assert.Empty(users.Users);
    }

    private static AuthTokenResponse CreateResponse()
    {
        return new AuthTokenResponse(
            "access",
            "refresh",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddDays(14),
            new AuthUserResponse("0x00000001", "admin", UserLevelType.Administrator.ToString(), true)
        );
    }

    private sealed class FakeAuthTokenService : IAuthTokenService
    {
        public AuthTokenResponse? LoginResponse { get; set; }
        public AuthTokenResponse? RefreshResponse { get; set; }
        public bool LogoutResponse { get; set; }

        public ValueTask<AuthTokenResponse?> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(LoginResponse);
        }

        public ValueTask<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(LogoutResponse);
        }

        public ValueTask<AuthTokenResponse?> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(RefreshResponse);
        }
    }

    private sealed class FakeUserService : IUserService
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _nextId = 1;

        public bool ThrowConflict { get; set; }
        public IReadOnlyCollection<UserEntity> Users => _users.Values.ToArray();

        public ValueTask<UserEntity?> ActivateAsync(string activationId, CancellationToken cancellationToken = default)
        {
            var normalizedActivationId = activationId.Trim();
            var user = _users.Values.FirstOrDefault(candidate => string.Equals(
                    candidate.ActivationId,
                    normalizedActivationId,
                    StringComparison.Ordinal
                )
            );

            if (user is null)
            {
                return ValueTask.FromResult<UserEntity?>(null);
            }

            user.IsActive = true;
            user.ActivationId = null;

            return ValueTask.FromResult<UserEntity?>(user);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.Count);
        }

        public ValueTask<UserEntity> CreateAsync(
            string username,
            string email,
            string password,
            UserLevelType level = UserLevelType.Player,
            bool isActive = true,
            string? activationId = null,
            CancellationToken cancellationToken = default
        )
        {
            if (ThrowConflict)
            {
                throw new InvalidOperationException($"User '{username}' already exists.");
            }

            var user = new UserEntity(
                new Serial(_nextId++),
                username,
                email,
                HashUtils.HashPassword(password),
                level,
                isActive,
                activationId
            );
            _users[user.Id] = user;

            return ValueTask.FromResult(user);
        }

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.Remove(id));
        }

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.GetValueOrDefault(id));
        }

        public ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(
                _users.Values.FirstOrDefault(user => string.Equals(
                        user.Username,
                        username,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            );
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

        public ValueTask<UserEntity?> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotSupportedException();
        }

        public ValueTask<bool> ResetPasswordAsync(
            Serial id,
            string newPassword,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_users.ContainsKey(id));
        }

        public ValueTask<UserEntity?> SetActiveAsync(Serial id, bool isActive, CancellationToken cancellationToken = default)
        {
            if (!_users.TryGetValue(id, out var user))
            {
                return ValueTask.FromResult<UserEntity?>(null);
            }

            user.IsActive = isActive;

            return ValueTask.FromResult<UserEntity?>(user);
        }

        public ValueTask<UserEntity?> UpdateAsync(
            Serial id,
            string email,
            UserLevelType level,
            CancellationToken cancellationToken = default
        )
        {
            if (!_users.TryGetValue(id, out var user))
            {
                return ValueTask.FromResult<UserEntity?>(null);
            }

            user.Email = email;
            user.Level = level;

            return ValueTask.FromResult<UserEntity?>(user);
        }

        public UserEntity Seed(string username, bool isActive, string? activationId)
        {
            var user = new UserEntity(
                new Serial(_nextId++),
                username,
                $"{username}@realm.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                isActive,
                activationId
            );
            _users[user.Id] = user;

            return user;
        }
    }
}
