namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetRegionDefinition
{
    public string Type { get; set; } = "";
    public string Map { get; set; } = "";
    public string Name { get; set; } = "";
    public int Priority { get; set; }
    public List<ServerAssetRectangle> Area { get; set; } = [];
    public ServerAssetWorldPoint? Entrance { get; set; }
    public ServerAssetWorldPoint? GoLocation { get; set; }
    public string Music { get; set; } = "";
}
