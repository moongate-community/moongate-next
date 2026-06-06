namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one row from data/components/doors.txt.
/// </summary>
public readonly record struct DoorComponentEntry
{
    public int Category { get; }

    public int Piece1 { get; }

    public int Piece2 { get; }

    public int Piece3 { get; }

    public int Piece4 { get; }

    public int Piece5 { get; }

    public int Piece6 { get; }

    public int Piece7 { get; }

    public int Piece8 { get; }

    public int FeatureMask { get; }

    public string Comment { get; }

    public DoorComponentEntry(
        int category,
        int piece1,
        int piece2,
        int piece3,
        int piece4,
        int piece5,
        int piece6,
        int piece7,
        int piece8,
        int featureMask,
        string comment
    )
    {
        Category = category;
        Piece1 = piece1;
        Piece2 = piece2;
        Piece3 = piece3;
        Piece4 = piece4;
        Piece5 = piece5;
        Piece6 = piece6;
        Piece7 = piece7;
        Piece8 = piece8;
        FeatureMask = featureMask;
        Comment = comment;
    }
}
