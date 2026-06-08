namespace Moongate.UO.Data.Data.Mobiles;

/// <summary>Current and maximum vital pools of a mobile.</summary>
public sealed class MobileResources
{
    /// <summary>Current hit points.</summary>
    public int Hits { get; set; }

    /// <summary>Maximum hit points.</summary>
    public int MaxHits { get; set; }

    /// <summary>Current mana.</summary>
    public int Mana { get; set; }

    /// <summary>Maximum mana.</summary>
    public int MaxMana { get; set; }

    /// <summary>Current stamina.</summary>
    public int Stamina { get; set; }

    /// <summary>Maximum stamina.</summary>
    public int MaxStamina { get; set; }
}
