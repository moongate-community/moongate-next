using System.Collections.Concurrent;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Data.Sessions;
using Moongate.Server.Interfaces.Sessions;

namespace Moongate.Server.Services.Sessions;

/// <summary>
/// In-memory <see cref="IGameLoginHandoffService" />: a process-level map of pending login handoffs,
/// pruned periodically through the timer wheel. Entries live for <see cref="TimeToLive" />.
/// </summary>
public sealed class GameLoginHandoffService : IGameLoginHandoffService
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PruneInterval = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<uint, GameLoginHandoff> _handoffs = new();
    private readonly Func<DateTimeOffset> _now;

    public GameLoginHandoffService(ITimerService? timerService = null, Func<DateTimeOffset>? now = null)
    {
        _now = now ?? (() => DateTimeOffset.UtcNow);

        timerService?.RegisterTimer("game_login_handoff_prune", PruneInterval, PruneExpired, PruneInterval, true);
    }

    /// <summary>Number of currently stored (not yet consumed or pruned) handoffs.</summary>
    public int Count => _handoffs.Count;

    /// <summary>Removes every handoff older than <see cref="TimeToLive" />. Invoked by the timer wheel.</summary>
    public void PruneExpired()
    {
        var now = _now();

        foreach (var pair in _handoffs)
        {
            if (now - pair.Value.CreatedAt > TimeToLive)
            {
                _handoffs.TryRemove(pair.Key, out _);
            }
        }
    }

    public void Store(uint sessionKey, ClientType clientType, ClientVersion? clientVersion)
        => _handoffs[sessionKey] = new(sessionKey, clientType, clientVersion, _now());

    public bool TryConsume(uint sessionKey, out GameLoginHandoff handoff)
    {
        if (_handoffs.TryRemove(sessionKey, out handoff!) && _now() - handoff.CreatedAt <= TimeToLive)
        {
            return true;
        }

        handoff = null!;

        return false;
    }
}
