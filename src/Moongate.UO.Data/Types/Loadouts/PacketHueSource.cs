namespace Moongate.UO.Data.Types.Loadouts;

/// <summary>
///     Declares which character-creation (0xF8) packet hue applies to a loadout equip entry.
/// </summary>
public enum PacketHueSource : byte
{
    /// <summary>No packet hue; the item keeps its template hue.</summary>
    None = 0,

    /// <summary>Apply the packet's shirt hue.</summary>
    Shirt = 1,

    /// <summary>Apply the packet's pants hue.</summary>
    Pants = 2
}
