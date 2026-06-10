namespace Moongate.UO.Data.Templates.Loot;

/// <summary>
/// Root YAML document for loot table files (<c>loot_tables</c> key).
/// </summary>
public sealed class LootTableTable
{
    public List<LootTableDefinition> LootTables { get; set; } = [];
}
