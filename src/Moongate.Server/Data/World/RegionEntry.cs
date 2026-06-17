using Moongate.Core.Geometry;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Server.Data.World;

/// <summary>
///     Represents one world region definition loaded from server asset data.
/// </summary>
public readonly record struct RegionEntry
{
    public RegionEntry(
        string type,
        int mapId,
        string map,
        string name,
        int priority,
        IReadOnlyList<RegionAreaEntry> area,
        string music,
        Point3D? entrance,
        Point3D? goLocation
    )
    {
        ArgumentNullException.ThrowIfNull(area);

        Type = type;
        MapId = mapId;
        Map = map;
        Name = name;
        Priority = priority;
        Area = Array.AsReadOnly(area.ToArray());
        Music = music;
        Entrance = entrance;
        GoLocation = goLocation;
    }

    public string Type { get; }

    public int MapId { get; }

    public string Map { get; }

    public string Name { get; }

    public int Priority { get; }

    public IReadOnlyList<RegionAreaEntry> Area { get; }

    public string Music { get; }

    public Point3D? Entrance { get; }

    public Point3D? GoLocation { get; }

    public RegionType Kind => RegionTypeParser.FromAssetType(Type);
}
