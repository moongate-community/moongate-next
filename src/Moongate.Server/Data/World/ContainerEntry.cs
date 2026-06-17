namespace Moongate.Server.Data.World;

/// <summary>
///     Represents one default container definition loaded from server asset data.
/// </summary>
public readonly record struct ContainerEntry
{
    public ContainerEntry(string id, int itemId, int width, int height, string name)
    {
        Id = id;
        ItemId = itemId;
        Width = width;
        Height = height;
        Name = name;
    }

    public string Id { get; }

    public int ItemId { get; }

    public int Width { get; }

    public int Height { get; }

    public string Name { get; }
}
