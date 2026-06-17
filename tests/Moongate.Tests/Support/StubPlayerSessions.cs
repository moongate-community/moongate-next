using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Core.Ids;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Tests.Support;

/// <summary>
///     IPlayerSessionService stub holding a fixed set of sessions returned by <see cref="GetAll" />.
///     Supports mutation via <see cref="Set" /> and <see cref="Clear" />; other members throw.
/// </summary>
public sealed class StubPlayerSessions : IPlayerSessionService
{
    private PlayerSession[] _sessions;

    public StubPlayerSessions(params PlayerSession[] sessions)
    {
        _sessions = sessions ?? [];
    }

    public int Count => _sessions.Length;

    public PlayerSession Authenticate(long sessionId, Serial userId, string username, DateTimeOffset authenticatedAt)
    {
        throw new NotSupportedException();
    }

    public bool Disconnect(long sessionId, DateTimeOffset disconnectedAt)
    {
        throw new NotSupportedException();
    }

    public PlayerSession EnterWorld(
        long sessionId,
        Serial characterSerial,
        Serial mobileSerial,
        DateTimeOffset enteredWorldAt
    )
    {
        throw new NotSupportedException();
    }

    public IReadOnlyCollection<PlayerSession> GetAll()
    {
        return _sessions;
    }

    public PlayerSession GetOrCreateConnected(long sessionId, string? remoteEndPoint, DateTimeOffset connectedAt)
    {
        throw new NotSupportedException();
    }

    public ValueEnumerable<FromArray<PlayerSession>, PlayerSession> Query()
    {
        throw new NotSupportedException();
    }

    public bool Remove(long sessionId)
    {
        throw new NotSupportedException();
    }

    public bool TryGetByMobileSerial(Serial mobileSerial, out PlayerSession session)
    {
        throw new NotSupportedException();
    }

    public bool TryGetBySessionId(long sessionId, out PlayerSession session)
    {
        throw new NotSupportedException();
    }

    public PlayerSession UpdateClient(long sessionId, ClientVersion? clientVersion = null, int? viewRange = null)
    {
        throw new NotSupportedException();
    }

    public void UpdateMovementState(long sessionId, byte moveSequence, long moveCredit, long moveTime)
    {
        throw new NotSupportedException();
    }

    public void Clear()
    {
        _sessions = [];
    }

    public void Set(params PlayerSession[] sessions)
    {
        _sessions = sessions ?? [];
    }
}
