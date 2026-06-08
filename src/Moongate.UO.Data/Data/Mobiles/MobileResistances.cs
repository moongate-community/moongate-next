namespace Moongate.UO.Data.Data.Mobiles;

/// <summary>Damage resistances of a mobile, as percentages.</summary>
public sealed class MobileResistances
{
    /// <summary>Physical resistance.</summary>
    public int Physical { get; set; }

    /// <summary>Fire resistance.</summary>
    public int Fire { get; set; }

    /// <summary>Cold resistance.</summary>
    public int Cold { get; set; }

    /// <summary>Poison resistance.</summary>
    public int Poison { get; set; }

    /// <summary>Energy resistance.</summary>
    public int Energy { get; set; }
}
