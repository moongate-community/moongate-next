namespace Moongate.UO.Data.Templates.Loot;

/// <summary>
/// A resolved amount range for a loot node. A fixed amount is represented as
/// <see cref="Min" /> == <see cref="Max" />.
/// </summary>
public sealed class LootAmount
{
    public int Min { get; }

    public int Max { get; }

    public LootAmount(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
