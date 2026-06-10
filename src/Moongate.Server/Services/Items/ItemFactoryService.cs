using Moongate.Core.Ids;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Server.Services.Items;

/// <summary>
/// Default item factory: maps a resolved item template onto a new persisted
/// <see cref="ItemEntity" />. Template metadata that has no entity field
/// (is_movable, params) is written into the entity custom properties.
/// </summary>
public sealed class ItemFactoryService : IItemFactoryService
{
    private readonly IItemTemplateService _templates;
    private readonly IItemService _items;

    public ItemFactoryService(IItemTemplateService templates, IItemService items)
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(items);

        _templates = templates;
        _items = items;
    }

    public async ValueTask<ItemEntity> CreateFromTemplateAsync(
        string templateId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        if (!_templates.TryGet(templateId, out var template))
        {
            throw new InvalidOperationException($"Item template '{templateId}' not found.");
        }

        if (template.IsAbstract)
        {
            throw new InvalidOperationException($"Item template '{templateId}' is abstract and cannot be instantiated.");
        }

        var item = new ItemEntity
        {
            Name = template.Name,
            ItemId = template.ItemId,
            Hue = (Hue)template.Hue,
            Weight = template.Weight,
            Amount = template.Amount,
            IsStackable = template.IsStackable,
            GumpId = template.GumpId,
            ScriptId = template.ScriptId,
            Rarity = template.Rarity,
            Visibility = template.Visibility
        };

        if (template.Value is not null)
        {
            item.BuyValue = template.Value.EffectiveBuy(template.Rarity);
            item.SellValue = template.Value.EffectiveSell(template.Rarity);
        }

        item.CustomProperties[ItemTemplateDefinition.ReservedIsMovableParamKey] = new CustomProperty
        {
            Type = CustomPropertyType.Boolean,
            BooleanValue = template.IsMovable
        };

        foreach (var (key, param) in template.Params)
        {
            item.CustomProperties[key] = ToCustomProperty(param);
        }

        return await _items.CreateAsync(item, cancellationToken);
    }

    private static CustomProperty ToCustomProperty(ItemTemplateParamDefinition param)
    {
        if (param.Type == ItemTemplateParamType.String)
        {
            return new CustomProperty
            {
                Type = CustomPropertyType.String,
                StringValue = param.Value
            };
        }

        return new CustomProperty
        {
            Type = CustomPropertyType.Integer,
            IntegerValue = ItemTemplateYamlLoader.ParseLong(param.Value)
        };
    }
}
