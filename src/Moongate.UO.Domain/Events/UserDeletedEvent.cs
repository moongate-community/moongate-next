using Moongate.Abstractions.Interfaces.Events;
using Moongate.Core.Ids;

namespace Moongate.UO.Domain.Events;

/// <summary>
/// Async event published after a user account has been deleted.
/// </summary>
public sealed record UserDeletedEvent : IAsyncEvent
{
    public Serial UserId { get; }
    public string Username { get; }
    public DateTimeOffset At { get; }

    public UserDeletedEvent(Serial userId, string username, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        UserId = userId;
        Username = username;
        At = at;
    }
}
