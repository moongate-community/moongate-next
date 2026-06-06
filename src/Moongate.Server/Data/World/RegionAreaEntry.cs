namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one rectangular area attached to a world region.
/// </summary>
public readonly record struct RegionAreaEntry
{
    public int X1 { get; }

    public int Y1 { get; }

    public int X2 { get; }

    public int Y2 { get; }

    public RegionAreaEntry(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }
}
