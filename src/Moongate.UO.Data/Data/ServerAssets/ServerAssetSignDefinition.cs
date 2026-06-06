namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetSignDefinition
{
    public int Map { get; set; }
    public int ItemId { get; set; }
    public List<int> Location { get; set; } = [];
    public string Text { get; set; } = "";
}
