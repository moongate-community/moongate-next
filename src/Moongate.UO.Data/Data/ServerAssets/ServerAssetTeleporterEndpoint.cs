namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetTeleporterEndpoint
{
    public string Map { get; set; } = "";
    public List<int> Loc { get; set; } = [];
}
