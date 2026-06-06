namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetSpawnEntry
{
    public string Name { get; set; } = "";
    public int MaxCount { get; set; }
    public int Probability { get; set; }
}
