using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateContentsSummary(
    string LootTemplate,
    string Generate,
    TimeSpan? RefillEvery,
    string RefillPolicy,
    string RefillScope
)
{
    public static ItemTemplateContentsSummary FromDefinition(ItemTemplateContentsDefinition contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        return new(
            contents.LootTemplate,
            contents.Generate.ToString(),
            contents.RefillEvery,
            contents.RefillPolicy.ToString(),
            contents.RefillScope.ToString()
        );
    }
}
