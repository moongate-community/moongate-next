using System.Collections.Concurrent;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Network.Client;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Services.Network.Internal;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Server.Services.Network;

/// <summary>
///     Thread-safe in-memory <see cref="ISessionService" /> backed by a concurrent dictionary.
/// </summary>
public sealed class SessionService : ISessionService, INetworkSessionManager
{
    private readonly IEventBusService? _eventBus;
    private readonly ConcurrentDictionary<long, GameSession> _sessions = new();

    public SessionService(IEventBusService? eventBus = null)
    {
        _eventBus = eventBus;
    }

    public IReadOnlyCollection<long> GetSessionIds()
    {
        return _sessions.Keys.ToArray();
    }

    public ValueEnumerable<FromArray<long>, long> QuerySessionIds()
    {
        return _sessions.Keys.ToArray().AsValueEnumerable();
    }

    public bool TryGetSession(long sessionId, out IGameSession session)
    {
        if (_sessions.TryGetValue(sessionId, out var found))
        {
            session = found;

            return true;
        }

        session = null!;

        return false;
    }

    public int Count => _sessions.Count;

    public void Clear()
    {
        _sessions.Clear();
    }

    public IReadOnlyCollection<GameSession> GetAll()
    {
        return _sessions.Values.ToArray();
    }

    public GameSession GetOrCreate(MoongateTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (_sessions.TryGetValue(client.SessionId, out var existing))
        {
            return existing;
        }

        var session = new GameSession(client);

        if (!_sessions.TryAdd(client.SessionId, session))
        {
            return _sessions.TryGetValue(client.SessionId, out existing) ? existing : GetOrCreate(client);
        }

        _eventBus?.Publish(
            new PlayerConnectedEvent(session.SessionId, client.RemoteEndPoint?.ToString(), DateTimeOffset.UtcNow)
        );

        return session;
    }

    public bool Remove(long sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            return false;
        }

        _eventBus?.Publish(
            new PlayerDisconnectedEvent(session.SessionId, session.Client.RemoteEndPoint?.ToString(), DateTimeOffset.UtcNow)
        );

        return true;
    }

    public bool TryGet(long sessionId, out GameSession session)
    {
        return _sessions.TryGetValue(sessionId, out session!);
    }
}
