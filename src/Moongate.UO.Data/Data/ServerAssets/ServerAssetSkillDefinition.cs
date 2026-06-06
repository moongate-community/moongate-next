namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetSkillDefinition
{
    public int SkillId { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public double StrScale { get; set; }
    public double DexScale { get; set; }
    public double IntScale { get; set; }
    public double StatTotal { get; set; }
    public double StrGain { get; set; }
    public double DexGain { get; set; }
    public double IntGain { get; set; }
    public double GainFactor { get; set; }
    public string ProfessionSkillName { get; set; } = "";
    public string PrimaryStat { get; set; } = "";
    public string SecondaryStat { get; set; } = "";
}
