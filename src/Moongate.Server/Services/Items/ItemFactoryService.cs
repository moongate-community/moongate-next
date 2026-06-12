using Moongate.Core.Ids;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
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

    public ValueTask<ItemEntity> CreateFromTemplateAsync(
        string templateId,
        CancellationToken cancellationToken = default
    )
        => CreateInternalAsync(templateId, null, cancellationToken);

    public ValueTask<ItemEntity> CreateFromTemplateAsync(
        string templateId,
        int amount,
        CancellationToken cancellationToken = default
    )
        => CreateInternalAsync(templateId, amount, cancellationToken);

    internal static int SelectTemplateItemId(ItemTemplateDefinition template, Func<int, int>? nextIndex = null)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (template.GraphicVariants.Count == 0)
        {
            return template.ItemId;
        }

        var count = template.GraphicVariants.Count + 1;
        var index = nextIndex?.Invoke(count) ?? Random.Shared.Next(count);

        if ((uint)index >= (uint)count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextIndex),
                $"Graphic variant selector returned index {index}, but valid indexes are 0 through {count - 1}."
            );
        }

        return index == 0
                   ? template.ItemId
                   : template.GraphicVariants[index - 1].ItemId;
    }

    private async ValueTask<ItemEntity> CreateInternalAsync(
        string templateId,
        int? amount,
        CancellationToken cancellationToken
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
            ItemId = SelectTemplateItemId(template),
            Hue = (Hue)template.Hue,
            Weight = template.Weight,
            Amount = amount ?? template.Amount,
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

        item.CustomProperties[ItemTemplateDefinitionKeys.TemplateId] = new()
        {
            Type = CustomPropertyType.String,
            StringValue = template.Id
        };

        item.CustomProperties[ItemTemplateDefinition.ReservedIsMovableParamKey] = new()
        {
            Type = CustomPropertyType.Boolean,
            BooleanValue = template.IsMovable
        };

        foreach (var (key, param) in template.Params)
        {
            item.CustomProperties[key] = TemplateParamConverter.ToCustomProperty(param);
        }

        return await _items.CreateAsync(item, cancellationToken);
    }
}
