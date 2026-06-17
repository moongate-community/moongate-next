namespace Moongate.Network.UO.Types.Player;

/// <summary>
///     The kind of status the client requests with the Get Player Status packet (0x34).
/// </summary>
public enum GetPlayerStatusType : byte
{
    GodClient = 0x00,
    BasicStatus = 0x04,  // expects a Player Status packet (0x11)
    RequestSkills = 0x05 // expects a Skills packet (0x3A)
}
