using DryIoc;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Persistence;
using Moongate.Server.Extensions.Users;
using Moongate.Server.Services.Users;
using Moongate.Tests.Support;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Events;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Tests.Server.Users;

public sealed class UserServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-users-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ActivateAsync_BlankActivationId_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.Null(await service.ActivateAsync("   "));
    }

    [Fact]
    public async Task ActivateAsync_KnownActivationId_ActivatesUserClearsActivationIdAndPublishesUpdate()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(9),
                "pending",
                "pending@realm.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                false,
                "token"
            )
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var user = await service.ActivateAsync(" token ");

        Assert.NotNull(user);
        Assert.True(user!.IsActive);
        Assert.Null(user.ActivationId);

        var stored = Assert.Single(access.Users);
        Assert.True(stored.IsActive);
        Assert.Null(stored.ActivationId);

        var updated = Assert.Single(bus.AsyncEvents.OfType<UserUpdatedEvent>());
        Assert.Equal(user.Id, updated.UserId);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_UnknownActivationId_ReturnsNullAndDoesNotPublish()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(9),
                "pending",
                "pending@realm.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                false,
                "token"
            )
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var user = await service.ActivateAsync("other-token");

        Assert.Null(user);
        Assert.Empty(bus.AsyncEvents);
    }

    [Fact]
    public async Task AddMoongateUsers_RegistersServiceAndUserPersistence()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "persistence:\n  enable_file_lock: false\n");

        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateUsers();
        container.AddMoongatePersistence(_dir);
        container.AddMoongateConfig(ConfigPath);

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var service = container.Resolve<IUserService>();

            var user = await service.CreateAsync("ContainerUser", "container@test.local", "secret");

            Assert.Equal(new Serial(1), user.Id);
            Assert.True(HashUtils.VerifyPassword("secret", user.Password));
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task CountAsync_ReturnsPersistedUserCount()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "one",
                "one@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        access.Add(
            new UserEntity(
                new Serial(2),
                "two",
                "two@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        var count = await service.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CreateAsync_BlankActivationId_PersistsNullActivationId()
    {
        var access = new FakeUserAccess();
        var service = new UserService(access, new CapturingEventBusService());

        var user = await service.CreateAsync(
            "Pending",
            "pending@test.local",
            "secret",
            activationId: "   "
        );

        Assert.Null(user.ActivationId);
        Assert.Null(Assert.Single(access.Users).ActivationId);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmailCaseInsensitive_Throws()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "first",
                "Taken@Realm.local",
                HashUtils.HashPassword("p"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateAsync("second", "taken@realm.local", "secret")
        );
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsernameCaseInsensitive_ThrowsAndDoesNotPublish()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(7),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("old"),
                UserLevelType.Player,
                true
            )
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateAsync("arthorius", "dupe@test.local", "secret")
        );

        Assert.Single(access.Users);
        Assert.Empty(bus.AsyncEvents);
    }

    [Fact]
    public async Task CreateAsync_HashesPasswordPersistsUserAndPublishesCreatedEvent()
    {
        var access = new FakeUserAccess();
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var user = await service.CreateAsync("Arthorius", "arthorius@test.local", "secret", UserLevelType.GameMaster);

        Assert.Equal(new Serial(1), user.Id);
        Assert.Equal("Arthorius", user.Username);
        Assert.Equal(UserLevelType.GameMaster, user.Level);
        Assert.True(user.IsActive);
        Assert.NotEqual("secret", user.Password);
        Assert.True(HashUtils.VerifyPassword("secret", user.Password));

        var stored = Assert.Single(access.Users);
        Assert.Equal(user.Id, stored.Id);
        Assert.Equal(user.Password, stored.Password);

        var created = Assert.Single(bus.AsyncEvents.OfType<UserCreatedEvent>());
        Assert.Equal(user.Id, created.UserId);
        Assert.Equal(user.Username, created.Username);
        Assert.Equal(user.Email, created.Email);
        Assert.Equal(user.Level, created.Level);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task CreateAsync_InvalidEmail_ThrowsArgumentException()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        await Assert.ThrowsAsync<ArgumentException>(async () => await service.CreateAsync("user", "not-an-email", "secret"));
    }

    [Fact]
    public async Task CreateAsync_WithActivationId_TrimsAndPersistsActivationId()
    {
        var access = new FakeUserAccess();
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var user = await service.CreateAsync(
            "Pending",
            "pending@test.local",
            "secret",
            UserLevelType.Player,
            false,
            "  activation-token  "
        );

        Assert.Equal("activation-token", user.ActivationId);
        Assert.Equal("activation-token", Assert.Single(access.Users).ActivationId);

        var created = Assert.Single(bus.AsyncEvents.OfType<UserCreatedEvent>());
        Assert.Equal("activation-token", created.ActivationId);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser_PublishesDeletedEvent_ReturnsTrue()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(11), "zed", "z@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var deleted = await service.DeleteAsync(new Serial(11));

        Assert.True(deleted);
        Assert.Empty(access.Users);
        var evt = Assert.Single(bus.AsyncEvents.OfType<UserDeletedEvent>());
        Assert.Equal(new Serial(11), evt.UserId);
    }

    [Fact]
    public async Task DeleteAsync_UnknownUser_ReturnsFalse()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.False(await service.DeleteAsync(new Serial(404)));
    }

    [Fact]
    public async Task GetByUsernameAsync_MatchesUsernameCaseInsensitively()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(42),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        var user = await service.GetByUsernameAsync("arthorius");

        Assert.NotNull(user);
        Assert.Equal(new Serial(42), user!.Id);
    }

    [Fact]
    public async Task ListAsync_PaginatesAndReportsTotalOrderedByUsername()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(3), "carol", "c@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        access.Add(
            new UserEntity(new Serial(1), "alice", "a@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        access.Add(
            new UserEntity(new Serial(2), "bob", "b@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        var service = new UserService(access, new CapturingEventBusService());

        var firstPage = await service.ListAsync(new PageRequest(1, 2, null));
        var secondPage = await service.ListAsync(new PageRequest(2, 2, null));

        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(["alice", "bob"], firstPage.Items.Select(u => u.Username));
        Assert.Equal(["carol"], secondPage.Items.Select(u => u.Username));
    }

    [Fact]
    public async Task ListAsync_SearchMatchesUsernameOrEmail_CaseInsensitive()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "Arthorius",
                "art@realm.local",
                HashUtils.HashPassword("p"),
                UserLevelType.Player,
                true
            )
        );
        access.Add(
            new UserEntity(
                new Serial(2),
                "Bob",
                "bob@elsewhere.local",
                HashUtils.HashPassword("p"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        var byName = await service.ListAsync(new PageRequest(1, 20, "arthor"));
        var byEmail = await service.ListAsync(new PageRequest(1, 20, "ELSEWHERE"));

        Assert.Equal("Arthorius", Assert.Single(byName.Items).Username);
        Assert.Equal("Bob", Assert.Single(byEmail.Items).Username);
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("Arthorius", "")]
    [InlineData("   ", "secret")]
    public async Task LoginAsync_BlankInput_ReturnsNull(string username, string password)
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        Assert.Null(await service.LoginAsync(username, password));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                false
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        Assert.Null(await service.LoginAsync("Arthorius", "secret"));
    }

    [Fact]
    public async Task LoginAsync_UnknownUsername_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.Null(await service.LoginAsync("ghost", "secret"));
    }

    [Fact]
    public async Task LoginAsync_ValidActiveCredentials_ReturnsUser()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        var user = await service.LoginAsync("arthorius", "secret");

        Assert.NotNull(user);
        Assert.Equal(new Serial(1), user!.Id);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(
                new Serial(1),
                "Arthorius",
                "art@test.local",
                HashUtils.HashPassword("secret"),
                UserLevelType.Player,
                true
            )
        );
        var service = new UserService(access, new CapturingEventBusService());

        Assert.Null(await service.LoginAsync("Arthorius", "wrong"));
    }

    [Fact]
    public async Task ResetPasswordAsync_EmptyPassword_Throws()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(9), "ivo", "i@realm.local", HashUtils.HashPassword("old"), UserLevelType.Player, true)
        );
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<ArgumentException>(async () => await service.ResetPasswordAsync(new Serial(9), "  "));
    }

    [Fact]
    public async Task ResetPasswordAsync_RehashesPassword_ReturnsTrue()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(9), "ivo", "i@realm.local", HashUtils.HashPassword("old"), UserLevelType.Player, true)
        );
        var service = new UserService(access, new CapturingEventBusService());

        var changed = await service.ResetPasswordAsync(new Serial(9), "fresh");

        Assert.True(changed);
        Assert.True(HashUtils.VerifyPassword("fresh", Assert.Single(access.Users).Password));
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownUser_ReturnsFalse()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.False(await service.ResetPasswordAsync(new Serial(404), "fresh"));
    }

    [Fact]
    public async Task SetActiveAsync_TogglesIsActiveAndPublishesUpdate()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(8), "gwen", "g@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var result = await service.SetActiveAsync(new Serial(8), false);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
        Assert.False(Assert.Single(access.Users).IsActive);
        Assert.Single(bus.AsyncEvents.OfType<UserUpdatedEvent>());
    }

    [Fact]
    public async Task SetActiveAsync_UnknownUser_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.Null(await service.SetActiveAsync(new Serial(404), false));
    }

    [Fact]
    public async Task UpdateAsync_ChangesEmailAndLevel_PublishesUpdatedEvent()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(5), "mara", "old@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var updated = await service.UpdateAsync(new Serial(5), "new@realm.local", UserLevelType.GameMaster);

        Assert.NotNull(updated);
        Assert.Equal("new@realm.local", updated!.Email);
        Assert.Equal(UserLevelType.GameMaster, updated.Level);
        Assert.Equal("new@realm.local", Assert.Single(access.Users).Email);
        Assert.Single(bus.AsyncEvents.OfType<UserUpdatedEvent>());
    }

    [Fact]
    public async Task UpdateAsync_EmailTakenByAnotherUser_Throws()
    {
        var access = new FakeUserAccess();
        access.Add(
            new UserEntity(new Serial(1), "a", "a@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        access.Add(
            new UserEntity(new Serial(2), "b", "b@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true)
        );
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateAsync(new Serial(2), "a@realm.local", UserLevelType.Player)
        );
    }

    [Fact]
    public async Task UpdateAsync_UnknownUser_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        var updated = await service.UpdateAsync(new Serial(999), "x@realm.local", UserLevelType.Player);

        Assert.Null(updated);
    }

    private sealed class CapturingEventBusService : IEventBusService
    {
        public List<IAsyncEvent> AsyncEvents { get; } = [];
        public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }
        public int CurrentTickQueueDepth => 0;

        public int DrainTickEvents(int maxItems)
        {
            return 0;
        }

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent
        {
        }

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
        {
            AsyncEvents.Add(evt);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserAccess : IAutoDataAccess<UserEntity, Serial>
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _nextId = 1;

        public IReadOnlyCollection<UserEntity> Users => _users.Values.ToArray();

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.Count);
        }

        public ValueTask<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IReadOnlyCollection<UserEntity>>(_users.Values.Select(Clone).ToArray());
        }

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.TryGetValue(id, out var user) ? Clone(user) : null);
        }

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new Serial(_nextId++));
        }

        public IQueryable<UserEntity> Query()
        {
            return _users.Values.Select(Clone).AsQueryable();
        }

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_users.Remove(id));
        }

        public ValueTask UpsertAsync(UserEntity entity, CancellationToken cancellationToken = default)
        {
            _users[entity.Id] = Clone(entity);

            return ValueTask.CompletedTask;
        }

        public void Add(UserEntity user)
        {
            _users[user.Id] = Clone(user);

            if (user.Id.Value >= _nextId)
            {
                _nextId = user.Id.Value + 1;
            }
        }

        private static UserEntity Clone(UserEntity user)
        {
            return new UserEntity(
                user.Id,
                user.Username,
                user.Email,
                user.Password,
                user.Level,
                user.IsActive,
                user.ActivationId
            );
        }
    }
}
