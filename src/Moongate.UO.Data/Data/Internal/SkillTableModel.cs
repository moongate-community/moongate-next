using Moongate.UO.Data.Data.Skills;

namespace Moongate.UO.Data.Data.Internal;

/// <summary>TOML root binding for <c>skills.toml</c> (array of tables <c>[[skill]]</c>).</summary>
public sealed class SkillTableModel
{
    public List<SkillInfo> Skill { get; set; } = [];
}
