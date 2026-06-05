using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Core.Utils;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Events;
using Moongate.UO.Domain.Interfaces.Services;
using Moongate.UO.Domain.Types;

namespace Moongate.Server.Services.Users;

/// <summary>
/// Default user service backed by auto-increment persistence.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IAutoDataAccess<UserEntity, Serial> _users;
    private readonly IEventBusService _eventBus;

    public UserService(IAutoDataAccess<UserEntity, Serial> users, IEventBusService eventBus)
    {
        _users = users;
        _eventBus = eventBus;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => _users.CountAsync(cancellationToken);

    public async ValueTask<UserEntity> CreateAsync(
        string username,
        string password,
        UserLevelType level = UserLevelType.Player,
        bool isActive = true,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedUsername = NormalizeUsername(username);

        if (await GetByUsernameAsync(normalizedUsername, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"User '{normalizedUsername}' already exists.");
        }

        var id = await _users.NextIdAsync(cancellationToken);
        var user = new UserEntity(id, normalizedUsername, HashUtils.HashPassword(password), level, isActive);

        await _users.UpsertAsync(user, cancellationToken);
        await _eventBus.PublishAsync(
            new UserCreatedEvent(user.Id, user.Username, user.Level, user.IsActive, DateTimeOffset.UtcNow),
            cancellationToken
        );

        return user;
    }

    public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => _users.GetByIdAsync(id, cancellationToken);

    public ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValueTask.FromResult<UserEntity?>(null);
        }

        var normalizedUsername = username.Trim();
        var user = _users
                   .Query()
                   .FirstOrDefault(u => string.Equals(u.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase));

        return ValueTask.FromResult(user);
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        }

        return username.Trim();
    }
}
