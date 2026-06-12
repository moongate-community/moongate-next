using Moongate.UO.Data.Types.Items;
using YamlDotNet.Serialization;

namespace Moongate.UO.Data.Templates.Items;

public sealed class ItemTemplateContentsDefinition
{
    [YamlMember(Alias = "loot_template")]
    public string LootTemplate { get; set; } = "";

    [YamlMember(Alias = "generate")]
    public ItemTemplateContentGenerateType Generate { get; set; } = ItemTemplateContentGenerateType.OnOpen;

    [YamlMember(Alias = "refill_every")]
    public TimeSpan? RefillEvery { get; set; }

    [YamlMember(Alias = "refill_policy")]
    public ItemTemplateContentRefillPolicy RefillPolicy { get; set; } = ItemTemplateContentRefillPolicy.WhenEmpty;

    [YamlMember(Alias = "refill_scope")]
    public ItemTemplateContentRefillScope RefillScope { get; set; } = ItemTemplateContentRefillScope.WorldOnly;
}
