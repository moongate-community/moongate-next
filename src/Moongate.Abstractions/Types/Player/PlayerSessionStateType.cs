namespace Moongate.Abstractions.Types.Player;

/// <summary>
///     Represents the logical lifecycle state of a connected player session.
/// </summary>
public enum PlayerSessionStateType
{
    Connected = 0,
    Authenticated = 1,
    InWorld = 2,
    Disconnected = 3
}
