namespace Moongate.Network.UO.Types.Login;

/// <summary>
/// Defines login denial reasons sent by packet 0x82.
/// </summary>
public enum LoginDeniedReasonType : byte
{
    InvalidCredentials = 0x00,
    AccountInUse = 0x01,
    AccountBlocked = 0x02,
    BadPassword = 0x03,
    Idle = 0xFE,
    BadCommunication = 0xFF
}
