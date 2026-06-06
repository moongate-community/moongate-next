namespace Moongate.Server.Data.World;

/// <summary>
/// Represents an immutable profession skill entry loaded from server asset data.
/// </summary>
public readonly record struct ProfessionSkillEntry
{
    public string Name { get; }

    public int Value { get; }

    public ProfessionSkillEntry(string name, int value)
    {
        Name = name;
        Value = value;
    }
}
