using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
/// Boot-time fail-fast validation for mobile templates against the item template
/// registry and the loot service. Any violation throws so the server refuses to start.
/// </summary>
public static class MobileTemplateValidator
{
    public static void Validate(
        IReadOnlyList<MobileTemplateDefinition> templates,
        IItemTemplateService items,
        ILootService loot
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(loot);

        foreach (var template in templates)
        {
            ValidateNotoriety(template);
            ValidateEquipment(template, items);
            ValidateBackpack(template, items);
            ValidateLootTables(template, loot);
            ValidateSkills(template);
        }
    }

    private static ItemTemplateDefinition ResolveItem(string mobileId, string itemId, IItemTemplateService items)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new InvalidOperationException($"Mobile template '{mobileId}' has an empty item reference.");
        }

        if (!items.TryGet(itemId, out var item))
        {
            throw new InvalidOperationException(
                $"Mobile template '{mobileId}' references unknown item template '{itemId}'."
            );
        }

        if (item.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Mobile template '{mobileId}' references abstract item template '{itemId}'."
            );
        }

        return item;
    }

    private static void ValidateBackpack(MobileTemplateDefinition template, IItemTemplateService items)
    {
        if (string.IsNullOrWhiteSpace(template.BackpackTemplate))
        {
            return;
        }

        var item = ResolveItem(template.Id, template.BackpackTemplate, items);

        if (item.Layer is null)
        {
            throw new InvalidOperationException(
                $"Mobile template '{template.Id}' backpack_template '{item.Id}' has no layer."
            );
        }

        if (item.Layer != ItemLayerType.Backpack)
        {
            throw new InvalidOperationException(
                $"Mobile template '{template.Id}' backpack_template '{item.Id}' is not a Backpack-layer item (layer {item.Layer})."
            );
        }
    }

    private static void ValidateEquipment(MobileTemplateDefinition template, IItemTemplateService items)
    {
        foreach (var entry in template.Equipment)
        {
            var item = ResolveItem(template.Id, entry.Item, items);

            if (item.Layer is null)
            {
                throw new InvalidOperationException(
                    $"Mobile template '{template.Id}' equips item '{item.Id}' which has no layer."
                );
            }
        }
    }

    private static void ValidateLootTables(MobileTemplateDefinition template, ILootService loot)
    {
        foreach (var lootTableId in template.LootTables)
        {
            if (!loot.Has(lootTableId))
            {
                throw new InvalidOperationException(
                    $"Mobile template '{template.Id}' references unknown loot table '{lootTableId}'."
                );
            }
        }
    }

    private static void ValidateNotoriety(MobileTemplateDefinition template)
    {
        if (template.Notoriety == NotorietyType.Invalid)
        {
            throw new InvalidOperationException($"Mobile template '{template.Id}' has Invalid notoriety.");
        }
    }

    private static void ValidateSkills(MobileTemplateDefinition template)
    {
        foreach (var skillName in template.Skills.Keys)
        {
            if (!Enum.TryParse<UOSkillName>(skillName, true, out _))
            {
                throw new InvalidOperationException(
                    $"Mobile template '{template.Id}' references unknown skill '{skillName}'."
                );
            }
        }
    }
}
