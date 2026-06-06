namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetDecorationDefinition
{
    public string Type { get; set; } = "";
    public int? ItemId { get; set; }
    public string Arguments { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ServerAssetDecorationPlacement> Placements { get; set; } = [];
}
