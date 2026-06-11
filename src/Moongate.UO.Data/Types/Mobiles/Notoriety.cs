namespace Moongate.UO.Data.Types.Mobiles;

/// <summary>
/// Reputation/standing of a mobile, driving name colour and attackability.
/// </summary>
public enum Notoriety : byte
{
    Invalid = 0x00,
    Innocent = 0x01,
    Friend = 0x02,
    CanBeAttacked = 0x03,
    Criminal = 0x04,
    Enemy = 0x05,
    Murdered = 0x06,
    Invulnerable = 0x07
}
