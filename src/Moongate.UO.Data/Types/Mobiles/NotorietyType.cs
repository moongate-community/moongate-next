namespace Moongate.UO.Data.Types.Mobiles;

/// <summary>
/// Notoriety / standing of a mobile, driving the client name colour and attackability.
/// </summary>
public enum NotorietyType : byte
{
    /// <summary>Invalid / across the server line — no colour.</summary>
    Invalid = 0,

    /// <summary>Innocent — blue.</summary>
    Innocent = 1,

    /// <summary>Guilded / ally — green.</summary>
    Friend = 2,

    /// <summary>Attackable but not criminal — gray.</summary>
    CanBeAttacked = 3,

    /// <summary>Criminal — gray.</summary>
    Criminal = 4,

    /// <summary>Enemy — orange.</summary>
    Enemy = 5,

    /// <summary>Murderer — red.</summary>
    Murdered = 6,

    /// <summary>
    /// Invulnerable (0x07): conventionally the yellow GM name in UO;
    /// here rendered translucent (0x4000-hue style) for "unknown use".
    /// </summary>
    Invulnerable = 7
}
