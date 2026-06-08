using Moongate.Abstractions.Interfaces.Events;
using Moongate.Core.Ids;
using Moongate.UO.Domain.Types;

namespace Moongate.UO.Domain.Events;

/// <summary>
/// Async event published after a user account's profile has been updated.
/// </summary>
public sealed record UserUpdatedEvent : IAsyncEvent
{
    public Serial UserId { get; }
    public string Username { get; }
    public string Email { get; }
    public UserLevelType Level { get; }
    public bool IsActive { get; }
    public DateTimeOffset At { get; }

    public UserUpdatedEvent(
        Serial userId,
        string username,
        string email,
        UserLevelType level,
        bool isActive,
        DateTimeOffset at
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        UserId = userId;
        Username = username;
        Email = email;
        Level = level;
        IsActive = isActive;
        At = at;
    }
}
