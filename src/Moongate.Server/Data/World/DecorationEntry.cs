using Moongate.Core.Geometry;

namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one decoration placement entry loaded from server asset data.
/// </summary>
public readonly record struct DecorationEntry
{
    public int MapId { get; }

    public string SourceGroup { get; }

    public string SourceFile { get; }

    public string TypeName { get; }

    public string Description { get; }

    public int ItemId { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public Point3D Location { get; }

    public Point3D? Target { get; }

    public string Extra { get; }

    public DecorationEntry(
        int mapId,
        string sourceGroup,
        string sourceFile,
        string typeName,
        string description,
        int itemId,
        IReadOnlyDictionary<string, string> parameters,
        Point3D location,
        Point3D? target,
        string extra
    )
    {
        ArgumentNullException.ThrowIfNull(parameters);

        MapId = mapId;
        SourceGroup = sourceGroup;
        SourceFile = sourceFile;
        TypeName = typeName;
        Description = description;
        ItemId = itemId;
        Parameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
        Location = location;
        Target = target;
        Extra = extra;
    }
}
