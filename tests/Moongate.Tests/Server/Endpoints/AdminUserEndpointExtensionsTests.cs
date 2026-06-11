using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Users;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminUserEndpointExtensionsTests
{
    private sealed class FakeUserService : IUserService
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _next = 1;

        public bool ThrowConflict { get; set; }
        public IReadOnlyCollection<UserEntity> Users => _users.Values.ToArray();

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Count);

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
                throw new InvalidOperationException($"Email '{email}' is already in use.");
            }

            var user = new UserEntity(new(_next++), username, email, HashUtils.HashPassword(password), level, isActive, activationId);
            _users[user.Id] = user;

            return ValueTask.FromResult(user);
        }

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Remove(id));

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.TryGetValue(id, out var user) ? user : null);

        public ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(
                _users.Values.FirstOrDefault(
                    u =>
                        string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
                )
            );

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
            => throw new NotSupportedException();

        public ValueTask<bool> ResetPasswordAsync(
            Serial id,
            string newPassword,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(_users.ContainsKey(id));

        public void Seed(UserEntity user)
            => _users[user.Id] = user;

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
    }

    [Fact]
    public async Task HandleCreateAsync_DuplicateEmail_ReturnsConflict()
    {
        var service = new FakeUserService { ThrowConflict = true };

        var result = await AdminUserEndpointExtensions.HandleCreateAsync(
                         service,
                         new() { Username = "x", Email = "dupe@x.local", Password = "secret", Level = "Player" },
                         CancellationToken.None
                     );

        Assert.IsType<Conflict<string>>(result);
    }

    [Fact]
    public async Task HandleCreateAsync_InvalidLevel_ReturnsBadRequest()
    {
        var service = new FakeUserService();

        var result = await AdminUserEndpointExtensions.HandleCreateAsync(
                         service,
                         new() { Username = "x", Email = "x@x.local", Password = "secret", Level = "Wizard" },
                         CancellationToken.None
                     );

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task HandleDeleteAsync_OtherUser_ReturnsNoContent()
    {
        var service = new FakeUserService();
        service.Seed(new(new(1), "alice", "a@x.local", "HASH", UserLevelType.Administrator, true));
        service.Seed(new(new(2), "bob", "b@x.local", "HASH", UserLevelType.Player, true));

        var result = await AdminUserEndpointExtensions.HandleDeleteAsync(
                         service,
                         Caller("0x00000001"),
                         "0x00000002",
                         CancellationToken.None
                     );

        Assert.IsType<NoContent>(result);
        Assert.Single(service.Users);
    }

    [Fact]
    public async Task HandleDeleteAsync_SelfDelete_ReturnsForbidden()
    {
        var service = new FakeUserService();
        service.Seed(new(new(1), "alice", "a@x.local", "HASH", UserLevelType.Administrator, true));

        var result = await AdminUserEndpointExtensions.HandleDeleteAsync(
                         service,
                         Caller("0x00000001"),
                         "0x00000001",
                         CancellationToken.None
                     );

        Assert.IsType<ForbidHttpResult>(result);
        Assert.Single(service.Users);
    }

    [Fact]
    public async Task HandleListAsync_ReturnsPagedSummaries_WithoutPasswords()
    {
        var service = new FakeUserService();
        service.Seed(new(new(1), "alice", "a@x.local", "HASH", UserLevelType.Administrator, true));

        var result = await AdminUserEndpointExtensions.HandleListAsync(service, 1, 20, null, CancellationToken.None);

        var ok = Assert.IsType<Ok<PagedResult<UserSummary>>>(result);
        Assert.Equal("alice", Assert.Single(ok.Value!.Items).Username);
    }

    [Fact]
    public async Task HandleSetActiveAsync_LockSelf_ReturnsForbidden()
    {
        var service = new FakeUserService();
        service.Seed(new(new(1), "alice", "a@x.local", "HASH", UserLevelType.Administrator, true));

        var result = await AdminUserEndpointExtensions.HandleSetActiveAsync(
                         service,
                         Caller("0x00000001"),
                         "0x00000001",
                         new() { IsActive = false },
                         CancellationToken.None
                     );

        Assert.IsType<ForbidHttpResult>(result);
    }

    [Fact]
    public async Task HandleUpdateAsync_UnknownUser_ReturnsNotFound()
    {
        var service = new FakeUserService();

        var result = await AdminUserEndpointExtensions.HandleUpdateAsync(
                         service,
                         Caller("0x00000001"),
                         "0x00000099",
                         new() { Email = "x@x.local", Level = "Player" },
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    private static ClaimsPrincipal Caller(string id)
        => new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, id)], "jwt"));
}
