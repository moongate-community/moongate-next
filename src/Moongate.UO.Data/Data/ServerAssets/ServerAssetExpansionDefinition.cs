namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetExpansionDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? RequiredClient { get; set; }
    public string ClientFlags { get; set; } = "";
    public Dictionary<string, bool> SupportedFeatures { get; set; } = [];
    public Dictionary<string, bool> MapSelectionFlags { get; set; } = [];
    public Dictionary<string, bool> CharacterListFlags { get; set; } = [];
    public Dictionary<string, bool> HousingFlags { get; set; } = [];
    public int MobileStatusVersion { get; set; }
}
