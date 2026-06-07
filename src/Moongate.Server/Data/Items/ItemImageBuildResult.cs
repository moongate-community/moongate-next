namespace Moongate.Server.Data.Items;

public sealed class ItemImageBuildResult
{
    public int MaxItemId { get; init; }

    public int Generated { get; init; }

    public int Cached { get; init; }

    public int Skipped { get; init; }

    public int Failed { get; init; }
}
