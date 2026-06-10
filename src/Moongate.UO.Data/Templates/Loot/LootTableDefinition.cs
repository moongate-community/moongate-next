namespace Moongate.UO.Data.Templates.Loot;

/// <summary>
/// A named loot table: a top-level group of nodes (its <see cref="Content" />)
/// resolved to produce items.
/// </summary>
public sealed class LootTableDefinition
{
    public string Id { get; set; } = "";

    public List<LootNode> Content { get; set; } = [];
}
