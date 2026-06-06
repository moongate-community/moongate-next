namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetSpawnDefinition
{
    public string Type { get; set; } = "";
    public string Guid { get; set; } = "";
    public string Name { get; set; } = "";
    public List<int> Location { get; set; } = [];
    public string Map { get; set; } = "";
    public int Count { get; set; }
    public TimeSpan MinDelay { get; set; }
    public TimeSpan MaxDelay { get; set; }
    public int Team { get; set; }
    public int HomeRange { get; set; }
    public int WalkingRange { get; set; }
    public List<ServerAssetSpawnEntry> Entries { get; set; } = [];
}
