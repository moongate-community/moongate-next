using Moongate.UO.Data.Types.Skills;

namespace Moongate.UO.Data.Data.Skills;

/// <summary>
///     Static reference data for a single UO skill: identity, stat scales/gains and its primary stats.
/// </summary>
public sealed class SkillInfo
{
    public int Id { get; set; }
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
    public StatType PrimaryStat { get; set; }
    public StatType SecondaryStat { get; set; }
}
