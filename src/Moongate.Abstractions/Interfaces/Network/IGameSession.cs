using System.Net;

namespace Moongate.Abstractions.Interfaces.Network;

/// <summary>
/// Curated, handler-safe view of an active network session: identity, connection endpoints, the
/// reconnect seed, and transport toggles. Exposes none of the transport internals (raw client,
/// pending byte buffer), so packet handlers can act on the session without server-layer lookups.
/// </summary>
public interface IGameSession
{
    /// <summary>Unique identifier of the session.</summary>
    long SessionId { get; }

    /// <summary>Remote endpoint of the connecting client, captured at session creation.</summary>
    IPEndPoint? ClientEndPoint { get; }

    /// <summary>Local endpoint the client connected to (the server IP:port that answered), captured at session creation.</summary>
    IPEndPoint? ServerEndPoint { get; }

    /// <summary>The connection seed captured on the game-server reconnect path; null otherwise.</summary>
    uint? Seed { get; }

    /// <summary>Enables UO transport compression for this session's outgoing packets (idempotent).</summary>
    void EnableCompression();
}
