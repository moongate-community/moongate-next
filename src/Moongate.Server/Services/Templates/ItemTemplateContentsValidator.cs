using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Services.Templates;

public static class ItemTemplateContentsValidator
{
    public static void Validate(
        IEnumerable<ItemTemplateDefinition> templates,
        LootTableRegistry registry,
        IItemService items
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var template in templates)
        {
            var contents = template.Contents;

            if (contents is null)
            {
                continue;
            }

            if (!registry.TryGet(contents.LootTemplate, out _))
            {
                throw new InvalidOperationException(
                    $"Item template '{template.Id}' references unknown contents loot_template '{contents.LootTemplate}'."
                );
            }

            if (contents.RefillEvery <= TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    $"Item template '{template.Id}' has invalid contents refill_every '{contents.RefillEvery}'."
                );
            }

            if (!template.IsAbstract && !items.IsContainer(template.ItemId))
            {
                throw new InvalidOperationException(
                    $"Item template '{template.Id}' defines contents but item_id '{template.ItemId}' is not a container."
                );
            }
        }
    }
}
