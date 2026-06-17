namespace Moongate.Server.Data.World;

/// <summary>
///     Represents one container layout definition loaded from server asset data.
/// </summary>
public readonly record struct ContainerLayoutEntry
{
    public ContainerLayoutEntry(int gumpId, IReadOnlyList<int> bounds, int dropSound, IReadOnlyList<int> itemIds)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        ArgumentNullException.ThrowIfNull(itemIds);

        GumpId = gumpId;
        Bounds = [.. bounds];
        DropSound = dropSound;
        ItemIds = [.. itemIds];
    }

    public int GumpId { get; }

    public IReadOnlyList<int> Bounds { get; }

    public int DropSound { get; }

    public IReadOnlyList<int> ItemIds { get; }
}
