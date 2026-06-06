using Moongate.Core.Geometry;

namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one world teleporter mapping loaded from server asset data.
/// </summary>
public readonly record struct TeleporterEntry
{
    public int SourceMapId { get; }

    public string SourceMapName { get; }

    public Point3D SourceLocation { get; }

    public int DestinationMapId { get; }

    public string DestinationMapName { get; }

    public Point3D DestinationLocation { get; }

    public bool Back { get; }

    public TeleporterEntry(
        int sourceMapId,
        string sourceMapName,
        Point3D sourceLocation,
        int destinationMapId,
        string destinationMapName,
        Point3D destinationLocation,
        bool back
    )
    {
        SourceMapId = sourceMapId;
        SourceMapName = sourceMapName;
        SourceLocation = sourceLocation;
        DestinationMapId = destinationMapId;
        DestinationMapName = destinationMapName;
        DestinationLocation = destinationLocation;
        Back = back;
    }
}
