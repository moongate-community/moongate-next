using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Server.Data.Templates;

public sealed record LootTemplateSummary(
    string Id,
    int RootNodeCount
)
{
    public static LootTemplateSummary FromDefinition(LootTableDefinition table)
    {
        ArgumentNullException.ThrowIfNull(table);

        return new LootTemplateSummary(table.Id, table.Content.Count);
    }
}
