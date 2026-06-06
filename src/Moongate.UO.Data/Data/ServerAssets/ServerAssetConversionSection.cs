namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetConversionSection
{
    public string Name { get; set; } = "";
    public List<ServerAssetConversionEntry> Entries { get; set; } = [];
}
