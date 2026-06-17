using Moongate.Core.Geometry;

namespace Moongate.Server.Data.World;

/// <summary>
///     Represents one sign placement entry loaded from server asset data.
/// </summary>
public readonly record struct SignEntry
{
    public SignEntry(int mapId, int sourceMapCode, int itemId, Point3D location, string text)
    {
        MapId = mapId;
        SourceMapCode = sourceMapCode;
        ItemId = itemId;
        Location = location;
        Text = text;
    }

    public int MapId { get; }

    public int SourceMapCode { get; }

    public int ItemId { get; }

    public Point3D Location { get; }

    public string Text { get; }
}
