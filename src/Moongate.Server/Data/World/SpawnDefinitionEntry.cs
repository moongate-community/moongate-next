using Moongate.Core.Geometry;
using Moongate.Server.Types.World;

namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one spawn definition loaded from ModernUO spawn YAML files.
/// </summary>
public readonly record struct SpawnDefinitionEntry
{
    public int MapId { get; }

    public string Map { get; }

    public string SourceGroup { get; }

    public string SourceFile { get; }

    public Guid Guid { get; }

    public SpawnDefinitionKind Kind { get; }

    public string Name { get; }

    public Point3D Location { get; }

    public int Count { get; }

    public TimeSpan MinDelay { get; }

    public TimeSpan MaxDelay { get; }

    public int Team { get; }

    public int HomeRange { get; }

    public int WalkingRange { get; }

    public IReadOnlyList<SpawnEntryDefinition> Entries { get; }

    public SpawnDefinitionEntry(
        int mapId,
        string map,
        string sourceGroup,
        string sourceFile,
        Guid guid,
        SpawnDefinitionKind kind,
        string name,
        Point3D location,
        int count,
        TimeSpan minDelay,
        TimeSpan maxDelay,
        int team,
        int homeRange,
        int walkingRange,
        IReadOnlyList<SpawnEntryDefinition> entries
    )
    {
        ArgumentNullException.ThrowIfNull(entries);

        MapId = mapId;
        Map = map;
        SourceGroup = sourceGroup;
        SourceFile = sourceFile;
        Guid = guid;
        Kind = kind;
        Name = name;
        Location = location;
        Count = count;
        MinDelay = minDelay;
        MaxDelay = maxDelay;
        Team = team;
        HomeRange = homeRange;
        WalkingRange = walkingRange;
        Entries = [.. entries];
    }
}
