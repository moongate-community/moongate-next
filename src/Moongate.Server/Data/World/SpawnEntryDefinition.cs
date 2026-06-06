namespace Moongate.Server.Data.World;

/// <summary>
/// Represents one entry inside a spawn definition.
/// </summary>
public readonly record struct SpawnEntryDefinition
{
    public string Name { get; }

    public int MaxCount { get; }

    public int Probability { get; }

    public SpawnEntryDefinition(string name, int maxCount, int probability)
    {
        Name = name;
        MaxCount = maxCount;
        Probability = probability;
    }
}
