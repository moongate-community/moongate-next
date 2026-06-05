using Moongate.Network.Client;
using Moongate.Server.Services.Network.Internal;

namespace Moongate.Server.Interfaces.Network;

/// <summary>
/// Registry of active network sessions keyed by session id.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Number of currently tracked sessions.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes every tracked session.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns a snapshot of all tracked sessions.
    /// </summary>
    /// <returns>The current sessions.</returns>
    IReadOnlyCollection<GameSession> GetAll();

    /// <summary>
    /// Returns the existing session for <paramref name="client" />, creating one if absent.
    /// </summary>
    /// <param name="client">Owning TCP client.</param>
    /// <returns>The session associated with the client.</returns>
    GameSession GetOrCreate(MoongateTCPClient client);

    /// <summary>
    /// Removes a session by its id.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns><c>true</c> when a session was removed; otherwise <c>false</c>.</returns>
    bool Remove(long sessionId);

    /// <summary>
    /// Tries to get a session by its id.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="session">The session when found.</param>
    /// <returns><c>true</c> when a session was found; otherwise <c>false</c>.</returns>
    bool TryGet(long sessionId, out GameSession session);
}
