using Moongate.Abstractions.Interfaces.Events;
using Moongate.Core.Ids;

namespace Moongate.Server.Data.Events;

/// <summary>
/// Raised after a player character has been placed into the world.
/// </summary>
public sealed record PlayerCharacterLoggedInEvent : ITickEvent
{
    public long SessionId { get; }
    public Serial AccountId { get; }
    public Serial CharacterId { get; }

    public PlayerCharacterLoggedInEvent(long sessionId, Serial accountId, Serial characterId)
    {
        SessionId = sessionId;
        AccountId = accountId;
        CharacterId = characterId;
    }
}
