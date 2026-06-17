using Moongate.UO.Data.Types.Loadouts;

namespace Moongate.UO.Data.Templates.Loadouts;

/// <summary>
///     One item entry in a starter loadout section, referencing an item template by id.
/// </summary>
public sealed class LoadoutItemEntry
{
    public string Template { get; set; } = "";

    /// <summary>Stack amount override; null keeps the template's amount.</summary>
    public int? Amount { get; set; }

    /// <summary>Which 0xF8 packet hue applies to this entry; equip entries only.</summary>
    public PacketHueSource PacketHue { get; set; } = PacketHueSource.None;
}
