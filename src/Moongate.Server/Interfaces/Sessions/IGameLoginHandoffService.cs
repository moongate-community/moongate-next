using Moongate.Abstractions.Data.Version;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Data.Sessions;

namespace Moongate.Server.Interfaces.Sessions;

/// <summary>
/// Bridges client metadata across the login-to-game-server redirect: the login connection stores a
/// handoff keyed by a generated session key, and the fresh game-server connection consumes it once.
/// </summary>
public interface IGameLoginHandoffService
{
    /// <summary>Stores a handoff for <paramref name="sessionKey" /> (overwrites any existing one).</summary>
    void Store(uint sessionKey, ClientType clientType, ClientVersion? clientVersion);

    /// <summary>
    /// Removes and returns the handoff for <paramref name="sessionKey" /> when present and not expired;
    /// returns false otherwise (unknown key, already consumed, or expired).
    /// </summary>
    bool TryConsume(uint sessionKey, out GameLoginHandoff handoff);
}
