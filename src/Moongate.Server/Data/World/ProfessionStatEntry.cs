namespace Moongate.Server.Data.World;

/// <summary>
/// Represents an immutable profession stat entry loaded from server asset data.
/// </summary>
public readonly record struct ProfessionStatEntry
{
    public string Type { get; }

    public int Value { get; }

    public ProfessionStatEntry(string type, int value)
    {
        Type = type;
        Value = value;
    }
}
