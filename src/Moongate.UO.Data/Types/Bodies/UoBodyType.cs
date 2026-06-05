namespace Moongate.UO.Data.Types.Bodies;

/// <summary>
/// Category of a UO body graphic, as classified by the body table.
/// </summary>
public enum UoBodyType : byte
{
    Empty,
    Monster,
    Sea,
    Animal,
    Human,
    Equipment
}
