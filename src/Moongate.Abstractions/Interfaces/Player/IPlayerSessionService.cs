using Moongate.Abstractions.Data.Player;
using Moongate.Core.Ids;
using Moongate.Abstractions.Data.Version;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Abstractions.Interfaces.Player;

/// <summary>
/// Manages logical player sessions associated with active network sessions.
/// </summary>
public interface IPlayerSessionService
{
    /// <summary>
    /// Number of currently tracked player sessions.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Creates a logical player session for a network session or returns the existing one.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="remoteEndPoint">Optional remote endpoint string.</param>
    /// <param name="connectedAt">Connection timestamp.</param>
    /// <returns>The logical player session.</returns>
    PlayerSession GetOrCreateConnected(long sessionId, string? remoteEndPoint, DateTimeOffset connectedAt);

    /// <summary>
    /// Marks a player session as authenticated.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <param name="username">Authenticated username.</param>
    /// <param name="authenticatedAt">Authentication timestamp.</param>
    /// <returns>The updated player session.</returns>
    PlayerSession Authenticate(long sessionId, string userId, string username, DateTimeOffset authenticatedAt);

    /// <summary>
    /// Attaches in-world character and mobile serials to a player session.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="characterSerial">Selected character serial.</param>
    /// <param name="mobileSerial">In-world mobile serial.</param>
    /// <param name="enteredWorldAt">World entry timestamp.</param>
    /// <returns>The updated player session.</returns>
    PlayerSession EnterWorld(
        long sessionId,
        Serial characterSerial,
        Serial mobileSerial,
        DateTimeOffset enteredWorldAt
    );

    /// <summary>
    /// Updates client metadata for a player session.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="clientVersion">Optional client version.</param>
    /// <param name="viewRange">Optional client view range.</param>
    /// <returns>The updated player session.</returns>
    PlayerSession UpdateClient(long sessionId, ClientVersion? clientVersion = null, int? viewRange = null);

    /// <summary>
    /// Marks a player session as disconnected and removes mobile indexes.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="disconnectedAt">Disconnect timestamp.</param>
    /// <returns><c>true</c> when a session was found and marked disconnected; otherwise <c>false</c>.</returns>
    bool Disconnect(long sessionId, DateTimeOffset disconnectedAt);

    /// <summary>
    /// Removes a logical player session completely.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <returns><c>true</c> when a session was removed; otherwise <c>false</c>.</returns>
    bool Remove(long sessionId);

    /// <summary>
    /// Tries to get a player session by network session id.
    /// </summary>
    /// <param name="sessionId">Network session identifier.</param>
    /// <param name="session">The player session when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGetBySessionId(long sessionId, out PlayerSession session);

    /// <summary>
    /// Tries to get a player session by in-world mobile serial.
    /// </summary>
    /// <param name="mobileSerial">In-world mobile serial.</param>
    /// <param name="session">The player session when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGetByMobileSerial(Serial mobileSerial, out PlayerSession session);

    /// <summary>
    /// Returns a snapshot of all tracked player sessions.
    /// </summary>
    /// <returns>The current player sessions.</returns>
    IReadOnlyCollection<PlayerSession> GetAll();

    /// <summary>
    /// Returns a ZLinq query over a snapshot of all tracked player sessions.
    /// </summary>
    ValueEnumerable<FromArray<PlayerSession>, PlayerSession> Query();
}
