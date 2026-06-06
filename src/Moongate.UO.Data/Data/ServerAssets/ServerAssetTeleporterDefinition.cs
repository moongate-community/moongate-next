namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetTeleporterDefinition
{
    public ServerAssetTeleporterEndpoint Src { get; set; } = new();
    public ServerAssetTeleporterEndpoint Dst { get; set; } = new();
    public bool Back { get; set; }
}
