using Moongate.Abstractions.Data.Version;
using Moongate.Network.UO.Types.Login;

namespace Moongate.Server.Data.Sessions;

/// <summary>
///     Metadata stashed when a client is redirected to the game server, recovered on reconnect by its
///     session key (the client echoes it back in the game-login packet).
/// </summary>
public sealed record GameLoginHandoff(
    uint SessionKey,
    ClientType ClientType,
    ClientVersion? ClientVersion,
    DateTimeOffset CreatedAt
);
