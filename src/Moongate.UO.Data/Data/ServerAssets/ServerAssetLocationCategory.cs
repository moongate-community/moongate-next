namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetLocationCategory
{
    public string Name { get; set; } = "";
    public List<ServerAssetLocationCategory> Categories { get; set; } = [];
    public List<ServerAssetLocationPoint> Locations { get; set; } = [];
}
