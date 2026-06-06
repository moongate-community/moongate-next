namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetDecorationPlacement
{
    public List<int> Location { get; set; } = [];
    public List<int> Target { get; set; } = [];
    public string Note { get; set; } = "";
}
