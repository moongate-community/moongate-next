using Moongate.Abstractions.Interfaces.Events;
using Moongate.Core.Ids;
using Moongate.Core.Types;

namespace Moongate.UO.Domain.Events;

/// <summary>
/// Async event published after a user account has been persisted.
/// </summary>
public sealed record UserCreatedEvent : IAsyncEvent
{
    public Serial UserId { get; }
    public string Username { get; }
    public string? Email { get; }
    public UserLevelType Level { get; }
    public bool IsActive { get; }
    public string? ActivationId { get; }
    public DateTimeOffset At { get; }

    public UserCreatedEvent(
        Serial userId,
        string username,
        UserLevelType level,
        bool isActive,
        DateTimeOffset at,
        string? email = null,
        string? activationId = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        UserId = userId;
        Username = username;
        Email = email;
        Level = level;
        IsActive = isActive;
        ActivationId = activationId;
        At = at;
    }
}
