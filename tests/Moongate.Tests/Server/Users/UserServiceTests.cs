using DryIoc;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Core.Utils;
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
using Moongate.UO.Domain.Types;

namespace Moongate.Tests.Server.Users;

public sealed class UserServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-users-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    private sealed class CapturingEventBusService : IEventBusService
    {
        public List<IAsyncEvent> AsyncEvents { get; } = [];
        public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }
        public int CurrentTickQueueDepth => 0;

        public int DrainTickEvents(int maxItems)
            => 0;

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent { }

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
        {
            AsyncEvents.Add(evt);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeUserAccess : IAutoDataAccess<UserEntity, Serial>
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _nextId = 1;

        public IReadOnlyCollection<UserEntity> Users => _users.Values.ToArray();

        public void Add(UserEntity user)
        {
            _users[user.Id] = Clone(user);

            if (user.Id.Value >= _nextId)
            {
                _nextId = user.Id.Value + 1;
            }
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Count);

        public ValueTask<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyCollection<UserEntity>>(_users.Values.Select(Clone).ToArray());

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.TryGetValue(id, out var user) ? Clone(user) : null);

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new Serial(_nextId++));

        public IQueryable<UserEntity> Query()
            => _users.Values.Select(Clone).AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Remove(id));

        public ValueTask UpsertAsync(UserEntity entity, CancellationToken cancellationToken = default)
        {
            _users[entity.Id] = Clone(entity);

            return ValueTask.CompletedTask;
        }

        private static UserEntity Clone(UserEntity user)
            => new(user.Id, user.Username, user.Email, user.Password, user.Level, user.IsActive);
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

            Assert.Equal(new(1), user.Id);
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
        access.Add(new(new(1), "one", "one@test.local", HashUtils.HashPassword("secret"), UserLevelType.Player, true));
        access.Add(new(new(2), "two", "two@test.local", HashUtils.HashPassword("secret"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var count = await service.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmailCaseInsensitive_Throws()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(1), "first", "Taken@Realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreateAsync("second", "taken@realm.local", "secret")
        );
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsernameCaseInsensitive_ThrowsAndDoesNotPublish()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(7), "Arthorius", "art@test.local", HashUtils.HashPassword("old"), UserLevelType.Player, true));
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreateAsync("arthorius", "dupe@test.local", "secret")
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

        Assert.Equal(new(1), user.Id);
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
    public async Task DeleteAsync_RemovesUser_PublishesDeletedEvent_ReturnsTrue()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(11), "zed", "z@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var deleted = await service.DeleteAsync(new(11));

        Assert.True(deleted);
        Assert.Empty(access.Users);
        var evt = Assert.Single(bus.AsyncEvents.OfType<UserDeletedEvent>());
        Assert.Equal(new(11), evt.UserId);
    }

    [Fact]
    public async Task DeleteAsync_UnknownUser_ReturnsFalse()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.False(await service.DeleteAsync(new(404)));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByUsernameAsync_MatchesUsernameCaseInsensitively()
    {
        var access = new FakeUserAccess();
        access.Add(
            new(new(42), "Arthorius", "art@test.local", HashUtils.HashPassword("secret"), UserLevelType.Player, true)
        );
        var service = new UserService(access, new CapturingEventBusService());

        var user = await service.GetByUsernameAsync("arthorius");

        Assert.NotNull(user);
        Assert.Equal(new(42), user!.Id);
    }

    [Fact]
    public async Task ListAsync_PaginatesAndReportsTotalOrderedByUsername()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(3), "carol", "c@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        access.Add(new(new(1), "alice", "a@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        access.Add(new(new(2), "bob", "b@x.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var firstPage = await service.ListAsync(new(1, 2, null));
        var secondPage = await service.ListAsync(new(2, 2, null));

        Assert.Equal(3, firstPage.TotalCount);
        Assert.Equal(["alice", "bob"], firstPage.Items.Select(u => u.Username));
        Assert.Equal(["carol"], secondPage.Items.Select(u => u.Username));
    }

    [Fact]
    public async Task ListAsync_SearchMatchesUsernameOrEmail_CaseInsensitive()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(1), "Arthorius", "art@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        access.Add(new(new(2), "Bob", "bob@elsewhere.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var byName = await service.ListAsync(new(1, 20, "arthor"));
        var byEmail = await service.ListAsync(new(1, 20, "ELSEWHERE"));

        Assert.Equal("Arthorius", Assert.Single(byName.Items).Username);
        Assert.Equal("Bob", Assert.Single(byEmail.Items).Username);
    }

    [Fact]
    public async Task ResetPasswordAsync_EmptyPassword_Throws()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(9), "ivo", "i@realm.local", HashUtils.HashPassword("old"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<ArgumentException>(async () => await service.ResetPasswordAsync(new(9), "  "));
    }

    [Fact]
    public async Task ResetPasswordAsync_RehashesPassword_ReturnsTrue()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(9), "ivo", "i@realm.local", HashUtils.HashPassword("old"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var changed = await service.ResetPasswordAsync(new(9), "fresh");

        Assert.True(changed);
        Assert.True(HashUtils.VerifyPassword("fresh", Assert.Single(access.Users).Password));
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownUser_ReturnsFalse()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.False(await service.ResetPasswordAsync(new(404), "fresh"));
    }

    [Fact]
    public async Task SetActiveAsync_TogglesIsActiveAndPublishesUpdate()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(8), "gwen", "g@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var result = await service.SetActiveAsync(new(8), false);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
        Assert.False(Assert.Single(access.Users).IsActive);
        Assert.Single(bus.AsyncEvents.OfType<UserUpdatedEvent>());
    }

    [Fact]
    public async Task SetActiveAsync_UnknownUser_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        Assert.Null(await service.SetActiveAsync(new(404), false));
    }

    [Fact]
    public async Task UpdateAsync_ChangesEmailAndLevel_PublishesUpdatedEvent()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(5), "mara", "old@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var updated = await service.UpdateAsync(new(5), "new@realm.local", UserLevelType.GameMaster);

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
        access.Add(new(new(1), "a", "a@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        access.Add(new(new(2), "b", "b@realm.local", HashUtils.HashPassword("p"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.UpdateAsync(new(2), "a@realm.local", UserLevelType.Player)
        );
    }

    [Fact]
    public async Task UpdateAsync_UnknownUser_ReturnsNull()
    {
        var service = new UserService(new FakeUserAccess(), new CapturingEventBusService());

        var updated = await service.UpdateAsync(new(999), "x@realm.local", UserLevelType.Player);

        Assert.Null(updated);
    }
}
