namespace Moongate.Server.Data.World;

/// <summary>
///     Represents an immutable profession definition loaded from server asset data.
/// </summary>
public readonly record struct ProfessionEntry
{
    public ProfessionEntry(
        string name,
        string trueName,
        int nameId,
        int descId,
        int desc,
        bool topLevel,
        int gump,
        string type,
        IReadOnlyList<ProfessionSkillEntry> skills,
        IReadOnlyList<ProfessionStatEntry> stats
    )
    {
        ArgumentNullException.ThrowIfNull(skills);
        ArgumentNullException.ThrowIfNull(stats);

        Name = name;
        TrueName = trueName;
        NameId = nameId;
        DescId = descId;
        Desc = desc;
        TopLevel = topLevel;
        Gump = gump;
        Type = type;
        Skills = Array.AsReadOnly(skills.ToArray());
        Stats = Array.AsReadOnly(stats.ToArray());
    }

    public string Name { get; }

    public string TrueName { get; }

    public int NameId { get; }

    public int DescId { get; }

    public int Desc { get; }

    public bool TopLevel { get; }

    public int Gump { get; }

    public string Type { get; }

    public IReadOnlyList<ProfessionSkillEntry> Skills { get; }

    public IReadOnlyList<ProfessionStatEntry> Stats { get; }
}
