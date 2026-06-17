namespace Moongate.UO.Data.Types.Maps;

/// <summary>
///     The kind of a world region, parsed from the server-asset region type string.
/// </summary>
public enum RegionType
{
    Unknown = 0,
    Base,
    GreenAcres,
    Guarded,
    Jail,
    Dungeon,
    NoHousing,
    Town
}
