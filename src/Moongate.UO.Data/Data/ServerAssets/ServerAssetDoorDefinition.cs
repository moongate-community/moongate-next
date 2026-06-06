namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetDoorDefinition
{
    public int Category { get; set; }
    public List<int> Pieces { get; set; } = [];
    public int FeatureMask { get; set; }
    public string Comment { get; set; } = "";
}
