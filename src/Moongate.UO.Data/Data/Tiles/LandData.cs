using Moongate.UO.Data.Types.Tiles;

namespace Moongate.UO.Data.Data.Tiles;

/// <summary>
/// Static properties of a single land tile, as read from <c>tiledata.mul</c>.
/// </summary>
public struct LandData
{
    public LandData(string name, UoTileFlag flags)
    {
        Name = name;
        Flags = flags;
    }

    public string Name { get; set; }

    public UoTileFlag Flags { get; set; }

    public override string ToString()
    {
        return $" {Name} ({Flags})";
    }
}
