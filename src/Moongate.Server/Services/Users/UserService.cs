using System.Net.Mail;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Core.Utils;
using Moongate.Persistence.Access;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.UO.Domain.Entities;
using Moongate.UO.Domain.Events;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Services.Users;

/// <summary>
/// Default user service backed by auto-increment persistence.
/// </summary>
public sealed class UserService : PaginatedEntityService<UserEntity, Serial>, IUserService
{
    private readonly IAutoDataAccess<UserEntity, Serial> _users;
    private readonly IEventBusService _eventBus;

    public UserService(IAutoDataAccess<UserEntity, Serial> users, IEventBusService eventBus)
        : base(users)
    {
        _users = users;
        _eventBus = eventBus;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => _users.CountAsync(cancellationToken);

    public async ValueTask<UserEntity?> ActivateAsync(string activationId, CancellationToken cancellationToken = default)
    {
        var normalizedActivationId = NormalizeActivationId(activationId);

        if (normalizedActivationId is null)
        {
            return null;
        }

        var user = _users
                   .Query()
                   .FirstOrDefault(
                       u => string.Equals(u.ActivationId, normalizedActivationId, StringComparison.Ordinal)
                   );

        if (user is null)
        {
            return null;
        }

        user.IsActive = true;
        user.ActivationId = null;

        await _users.UpsertAsync(user, cancellationToken);
        await _eventBus.PublishAsync(
            new UserUpdatedEvent(user.Id, user.Username, user.Email, user.Level, user.IsActive, DateTimeOffset.UtcNow),
            cancellationToken
        );

        return user;
    }

    public async ValueTask<UserEntity> CreateAsync(
        string username,
        string email,
        string password,
        UserLevelType level = UserLevelType.Player,
        bool isActive = true,
        string? activationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedUsername = NormalizeUsername(username);
        var normalizedEmail = NormalizeEmail(email);
        var normalizedActivationId = NormalizeActivationId(activationId);

        if (await GetByUsernameAsync(normalizedUsername, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"User '{normalizedUsername}' already exists.");
        }

        await EnsureEmailIsFreeAsync(normalizedEmail, null, cancellationToken);

        var id = await _users.NextIdAsync(cancellationToken);
        var user =
            new UserEntity(
                id,
                normalizedUsername,
                normalizedEmail,
                HashUtils.HashPassword(password),
                level,
                isActive,
                normalizedActivationId
            );

        await _users.UpsertAsync(user, cancellationToken);
        await _eventBus.PublishAsync(
            new UserCreatedEvent(
                user.Id,
                user.Username,
                user.Level,
                user.IsActive,
                DateTimeOffset.UtcNow,
                user.Email,
                user.ActivationId
            ),
            cancellationToken
        );

        return user;
    }

    public async ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var removed = await _users.RemoveAsync(id, cancellationToken);

        if (removed)
        {
            await _eventBus.PublishAsync(
                new UserDeletedEvent(user.Id, user.Username, DateTimeOffset.UtcNow),
                cancellationToken
            );
        }

        return removed;
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

        if (user is null || !user.IsActive || !HashUtils.VerifyPassword(password, user.Password))
        {
            return null;
        }

        return user;
    }

    public async ValueTask<bool> ResetPasswordAsync(
        Serial id,
        string newPassword,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(newPassword));
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.Password = HashUtils.HashPassword(newPassword);

        await _users.UpsertAsync(user, cancellationToken);

        return true;
    }

    public async ValueTask<UserEntity?> SetActiveAsync(
        Serial id,
        bool isActive,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.IsActive = isActive;

        await _users.UpsertAsync(user, cancellationToken);
        await _eventBus.PublishAsync(
            new UserUpdatedEvent(user.Id, user.Username, user.Email, user.Level, user.IsActive, DateTimeOffset.UtcNow),
            cancellationToken
        );

        return user;
    }

    public async ValueTask<UserEntity?> UpdateAsync(
        Serial id,
        string email,
        UserLevelType level,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var normalizedEmail = NormalizeEmail(email);

        await EnsureEmailIsFreeAsync(normalizedEmail, id, cancellationToken);

        user.Email = normalizedEmail;
        user.Level = level;

        await _users.UpsertAsync(user, cancellationToken);
        await _eventBus.PublishAsync(
            new UserUpdatedEvent(user.Id, user.Username, user.Email, user.Level, user.IsActive, DateTimeOffset.UtcNow),
            cancellationToken
        );

        return user;
    }

    protected override IQueryable<UserEntity> ApplyOrder(IQueryable<UserEntity> query)
        => query.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase);

    protected override IQueryable<UserEntity> ApplySearch(IQueryable<UserEntity> query, string term)
        => query.Where(
            u =>
                u.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
        );

    private async ValueTask EnsureEmailIsFreeAsync(string email, Serial? excludingId, CancellationToken cancellationToken)
    {
        var hasExclusion = excludingId.HasValue;
        var excludedSerial = excludingId.GetValueOrDefault();
        var clash = _users
                    .Query()
                    .FirstOrDefault(
                        u =>
                            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) &&
                            (!hasExclusion || u.Id != excludedSerial)
                    );

        if (clash is not null)
        {
            throw new InvalidOperationException($"Email '{email}' is already in use.");
        }

        await ValueTask.CompletedTask;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        var trimmed = email.Trim();

        if (!MailAddress.TryCreate(trimmed, out _))
        {
            throw new ArgumentException($"Email '{trimmed}' is not a valid address.", nameof(email));
        }

        return trimmed;
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        }

        return username.Trim();
    }

    private static string? NormalizeActivationId(string? activationId)
    {
        var normalized = activationId?.Trim();

        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
