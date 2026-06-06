namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetContainerLayoutDefinition
{
    public int GumpId { get; set; }
    public List<int> Bounds { get; set; } = [];
    public int DropSound { get; set; }
    public List<int> ItemIds { get; set; } = [];
}
