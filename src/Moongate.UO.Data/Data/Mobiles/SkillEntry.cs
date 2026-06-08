using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Data.Mobiles;

/// <summary>A mobile's progress in a single skill.</summary>
public sealed class SkillEntry
{
    /// <summary>Effective skill value (tenths of a percent, 0–1000 = 0.0–100.0).</summary>
    public double Value { get; set; }

    /// <summary>Base skill value before any temporary modifiers.</summary>
    public double Base { get; set; }

    /// <summary>Per-skill cap.</summary>
    public int Cap { get; set; }

    /// <summary>Raise/lower/lock state for this skill.</summary>
    public UOSkillLock Lock { get; set; }
}
