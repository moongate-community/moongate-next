using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Ids;
using Moongate.Server.Data.Events;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Server.Services.Player;

/// <summary>
/// Thread-safe in-memory logical player session service.
/// </summary>
public sealed class PlayerSessionService
    : IPlayerSessionService, ITickEventHandler<PlayerConnectedEvent>, ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly Lock _sync = new();
    private readonly Dictionary<long, PlayerSession> _sessions = [];
    private readonly Dictionary<Serial, long> _sessionsByMobileSerial = [];

    public int Count
    {
        get
        {
            lock (_sync)
            {
                return _sessions.Count;
            }
        }
    }

    public PlayerSession Authenticate(long sessionId, Serial userId, string username, DateTimeOffset authenticatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        lock (_sync)
        {
            var session = GetRequiredSession(sessionId);
            session.UserId = userId;
            session.Username = username;
            session.AuthenticatedAt = authenticatedAt;
            session.State = PlayerSessionStateType.Authenticated;

            return Copy(session);
        }
    }

    public bool Disconnect(long sessionId, DateTimeOffset disconnectedAt)
    {
        lock (_sync)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return false;
            }

            if (session.MobileSerial is { } mobileSerial)
            {
                _sessionsByMobileSerial.Remove(mobileSerial);
            }

            session.DisconnectedAt = disconnectedAt;
            session.State = PlayerSessionStateType.Disconnected;

            return true;
        }
    }

    public PlayerSession EnterWorld(
        long sessionId,
        Serial characterSerial,
        Serial mobileSerial,
        DateTimeOffset enteredWorldAt
    )
    {
        lock (_sync)
        {
            var session = GetRequiredSession(sessionId);

            if (session.MobileSerial is { } previousMobileSerial)
            {
                _sessionsByMobileSerial.Remove(previousMobileSerial);
            }

            session.CharacterSerial = characterSerial;
            session.MobileSerial = mobileSerial;
            session.EnteredWorldAt = enteredWorldAt;
            session.State = PlayerSessionStateType.InWorld;
            _sessionsByMobileSerial[mobileSerial] = sessionId;

            return Copy(session);
        }
    }

    public IReadOnlyCollection<PlayerSession> GetAll()
    {
        lock (_sync)
        {
            return _sessions.Values.Select(Copy).ToArray();
        }
    }

    public PlayerSession GetOrCreateConnected(long sessionId, string? remoteEndPoint, DateTimeOffset connectedAt)
    {
        lock (_sync)
        {
            if (_sessions.TryGetValue(sessionId, out var existing))
            {
                return Copy(existing);
            }

            var session = new PlayerSession
            {
                SessionId = sessionId,
                RemoteEndPoint = remoteEndPoint,
                ConnectedAt = connectedAt,
                State = PlayerSessionStateType.Connected
            };

            _sessions.Add(sessionId, session);

            return Copy(session);
        }
    }

    public void Handle(PlayerConnectedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        GetOrCreateConnected(evt.SessionId, evt.RemoteEndPoint, evt.At);
    }

    public void Handle(PlayerDisconnectedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        Disconnect(evt.SessionId, evt.At);
    }

    public ValueEnumerable<FromArray<PlayerSession>, PlayerSession> Query()
        => GetAll().ToArray().AsValueEnumerable();

    public bool Remove(long sessionId)
    {
        lock (_sync)
        {
            if (!_sessions.Remove(sessionId, out var session))
            {
                return false;
            }

            if (session.MobileSerial is { } mobileSerial)
            {
                _sessionsByMobileSerial.Remove(mobileSerial);
            }

            return true;
        }
    }

    public bool TryGetByMobileSerial(Serial mobileSerial, out PlayerSession session)
    {
        lock (_sync)
        {
            if (_sessionsByMobileSerial.TryGetValue(mobileSerial, out var sessionId) &&
                _sessions.TryGetValue(sessionId, out var found))
            {
                session = Copy(found);

                return true;
            }

            session = null!;

            return false;
        }
    }

    public bool TryGetBySessionId(long sessionId, out PlayerSession session)
    {
        lock (_sync)
        {
            if (_sessions.TryGetValue(sessionId, out var found))
            {
                session = Copy(found);

                return true;
            }

            session = null!;

            return false;
        }
    }

    public PlayerSession UpdateClient(long sessionId, ClientVersion? clientVersion = null, int? viewRange = null)
    {
        lock (_sync)
        {
            var session = GetRequiredSession(sessionId);

            if (clientVersion is not null)
            {
                session.ClientVersion = clientVersion;
            }

            if (viewRange is not null)
            {
                session.ViewRange = viewRange;
            }

            return Copy(session);
        }
    }

    public void UpdateMovementState(long sessionId, byte moveSequence, long moveCredit, long moveTime)
    {
        lock (_sync)
        {
            var session = GetRequiredSession(sessionId);

            session.MoveSequence = moveSequence;
            session.MoveCredit = moveCredit;
            session.MoveTime = moveTime;
        }
    }

    private static PlayerSession Copy(PlayerSession session)
        => new()
        {
            SessionId = session.SessionId,
            RemoteEndPoint = session.RemoteEndPoint,
            UserId = session.UserId,
            Username = session.Username,
            CharacterSerial = session.CharacterSerial,
            MobileSerial = session.MobileSerial,
            State = session.State,
            ConnectedAt = session.ConnectedAt,
            AuthenticatedAt = session.AuthenticatedAt,
            EnteredWorldAt = session.EnteredWorldAt,
            DisconnectedAt = session.DisconnectedAt,
            ViewRange = session.ViewRange,
            ClientVersion = session.ClientVersion,
            PingSequence = session.PingSequence,
            MoveSequence = session.MoveSequence,
            MoveCredit = session.MoveCredit,
            MoveTime = session.MoveTime
        };

    private PlayerSession GetRequiredSession(long sessionId)
        => _sessions.TryGetValue(sessionId, out var session)
               ? session
               : throw new InvalidOperationException($"Player session '{sessionId}' does not exist.");
}
