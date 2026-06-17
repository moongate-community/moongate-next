namespace Moongate.UO.Data.Templates.Loot;

/// <summary>
///     One node in a loot tree. Exactly one of <see cref="Item" />,
///     <see cref="Category" />, <see cref="PickOneOf" /> or <see cref="Group" />
///     identifies the node kind (enforced at boot validation).
/// </summary>
public sealed class LootNode
{
    /// <summary>Item template id to produce.</summary>
    public string? Item { get; set; }

    /// <summary>Item template tag; a random matching template is produced.</summary>
    public string? Category { get; set; }

    /// <summary>Children from which exactly one is chosen (weighted).</summary>
    public List<LootNode>? PickOneOf { get; set; }

    /// <summary>Children that are all resolved.</summary>
    public List<LootNode>? Group { get; set; }

    /// <summary>Probability (0..1) that this node is rolled at all.</summary>
    public double Chance { get; set; } = 1.0;

    /// <summary>Produced amount; null means 1. Ignored on pick_one_of/group.</summary>
    public LootAmount? Amount { get; set; }

    /// <summary>Relative weight; only meaningful as a direct child of pick_one_of.</summary>
    public int Weight { get; set; } = 1;
}
