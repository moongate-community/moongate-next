using Moongate.Core.Geometry;

namespace Moongate.Server.Data.World;

/// <summary>
/// Represents a flattened world location catalog entry.
/// </summary>
public readonly record struct WorldLocationEntry
{
    public int MapId { get; }

    public string MapName { get; }

    public string CategoryPath { get; }

    public string Name { get; }

    public Point3D Location { get; }

    public WorldLocationEntry(
        int mapId,
        string mapName,
        string categoryPath,
        string name,
        Point3D location
    )
    {
        MapId = mapId;
        MapName = mapName;
        CategoryPath = categoryPath;
        Name = name;
        Location = location;
    }
}
