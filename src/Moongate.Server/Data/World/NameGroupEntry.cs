namespace Moongate.Server.Data.World;

/// <summary>
/// Represents an immutable name group loaded from server asset data.
/// </summary>
public readonly record struct NameGroupEntry
{
    public string Type { get; }

    public IReadOnlyList<string> Names { get; }

    public NameGroupEntry(string type, IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        Type = type;
        Names = Array.AsReadOnly(names.ToArray());
    }
}
