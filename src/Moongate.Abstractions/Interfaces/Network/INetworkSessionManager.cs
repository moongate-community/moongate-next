using ZLinq;
using ZLinq.Linq;

namespace Moongate.Abstractions.Interfaces.Network;

/// <summary>
///     Read-only view of active network sessions for packet handlers and plugins.
/// </summary>
public interface INetworkSessionManager
{
    /// <summary>
    ///     Number of currently tracked sessions.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Returns a snapshot of active session identifiers.
    /// </summary>
    IReadOnlyCollection<long> GetSessionIds();

    /// <summary>
    ///     Returns a ZLinq query over a snapshot of active session identifiers.
    /// </summary>
    ValueEnumerable<FromArray<long>, long> QuerySessionIds();

    /// <summary>
    ///     Tries to get the handler-safe view of an active session by its id.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="session">The session when found.</param>
    /// <returns><c>true</c> when a session was found; otherwise <c>false</c>.</returns>
    bool TryGetSession(long sessionId, out IGameSession session);
}
