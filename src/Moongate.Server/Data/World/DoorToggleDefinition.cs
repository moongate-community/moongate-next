using Moongate.Core.Geometry;

namespace Moongate.Server.Data.World;

/// <summary>
///     Precomputed toggle metadata for a concrete door item id.
/// </summary>
public readonly record struct DoorToggleDefinition
{
    public DoorToggleDefinition(int currentItemId, int nextItemId, bool isClosed, Point3D offset)
    {
        CurrentItemId = currentItemId;
        NextItemId = nextItemId;
        IsClosed = isClosed;
        Offset = offset;
    }

    public int CurrentItemId { get; }

    public int NextItemId { get; }

    public bool IsClosed { get; }

    public Point3D Offset { get; }
}
