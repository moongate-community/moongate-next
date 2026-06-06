namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetProfession
{
    public string Name { get; set; } = "";
    public string TrueName { get; set; } = "";
    public int NameId { get; set; }
    public int DescId { get; set; }
    public int Desc { get; set; }
    public bool TopLevel { get; set; }
    public int Gump { get; set; }
    public string Type { get; set; } = "";
    public List<ServerAssetProfessionSkill> Skills { get; set; } = [];
    public List<ServerAssetProfessionStat> Stats { get; set; } = [];
}
