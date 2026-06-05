using Moongate.UO.Data.Data.Skills;

namespace Moongate.UO.Data.Interfaces.Skills;

/// <summary>Provides access to the UO skill reference table.</summary>
public interface ISkillDataStore
{
    /// <summary>All loaded skills.</summary>
    IReadOnlyList<SkillInfo> Skills { get; }

    /// <summary>Number of loaded skills.</summary>
    int Count { get; }

    /// <summary>Returns the skill with the given id, or <c>null</c>.</summary>
    /// <param name="skillId">Skill id.</param>
    SkillInfo? GetById(int skillId);

    /// <summary>Returns the skill with the given name (case-insensitive), or <c>null</c>.</summary>
    /// <param name="name">Skill name.</param>
    SkillInfo? GetByName(string name);
}
