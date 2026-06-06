using Moongate.UO.Data.Data.Skills;

namespace Moongate.UO.Data.Data.Internal;

/// <summary>YAML root binding for <c>skills.yaml</c>.</summary>
public sealed class SkillTableModel
{
    public List<SkillInfo> Skill { get; set; } = [];
}
