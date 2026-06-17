namespace Moongate.Network.UO.Types.Login;

/// <summary>
///     Defines advertised client families.
/// </summary>
[Flags]
public enum ClientType
{
    None = 0x00,
    Classic = 0x01,
    Uotd = 0x02,
    KingdomReborn = 0x04,
    StygianAbyss = 0x08
}
