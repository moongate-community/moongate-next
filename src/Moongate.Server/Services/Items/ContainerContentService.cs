using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Properties;

namespace Moongate.Server.Services.Items;

public sealed class ContainerContentService : IContainerContentService
{
    private const int FallbackStartX = 20;
    private const int FallbackStartY = 20;
    private const int GridStep = 36;
    private const int GridColumns = 5;
    private readonly IContainerDataService _containers;
    private readonly IItemService _items;
    private readonly ILootService _loot;

    private readonly IItemTemplateService _templates;

    public ContainerContentService(
        IItemTemplateService templates,
        IItemService items,
        ILootService loot,
        IContainerDataService containers
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(loot);
        ArgumentNullException.ThrowIfNull(containers);

        _templates = templates;
        _items = items;
        _loot = loot;
        _containers = containers;
    }

    public async Task EnsureContentsAsync(ItemEntity container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (!_items.IsContainer(container) ||
            container.ParentContainerId != Serial.Zero ||
            container.EquippedMobileId != Serial.Zero)
        {
            return;
        }

        if (!TryGetStringProperty(container, ItemTemplateDefinitionKeys.TemplateId, out var templateId) ||
            !_templates.TryGet(templateId, out var template) ||
            template.Contents is null)
        {
            return;
        }

        var contents = template.Contents;
        var hasGenerated = TryGetIntegerProperty(
            container,
            ItemTemplateDefinitionKeys.ContentsGeneratedAt,
            out _
        );

        if (hasGenerated && contents.RefillEvery is null)
        {
            return;
        }

        if (container.ContainedItemIds.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var nowUnixMilliseconds = now.ToUnixTimeMilliseconds();

        if (contents.RefillEvery is not null &&
            TryGetIntegerProperty(container, ItemTemplateDefinitionKeys.ContentsNextRefillAt, out var nextRefillAt) &&
            nowUnixMilliseconds < nextRefillAt)
        {
            return;
        }

        var generatedItems = await _loot.GenerateAsync(contents.LootTemplate, cancellationToken);

        for (var index = 0; index < generatedItems.Count; index++)
        {
            var position = GetPosition(container, index);

            await _items.AddItemAsync(container, generatedItems[index], position, cancellationToken);
        }

        SetIntegerProperty(container, ItemTemplateDefinitionKeys.ContentsGeneratedAt, nowUnixMilliseconds);

        if (contents.RefillEvery is not null)
        {
            SetIntegerProperty(
                container,
                ItemTemplateDefinitionKeys.ContentsNextRefillAt,
                now.Add(contents.RefillEvery.Value).ToUnixTimeMilliseconds()
            );
        }
    }

    private Point2D GetPosition(ItemEntity container, int index)
    {
        var startX = FallbackStartX;
        var startY = FallbackStartY;

        if (container.GumpId is { } gumpId)
        {
            var layout = _containers.GetAllLayouts().FirstOrDefault(entry => entry.GumpId == gumpId);

            if (layout.Bounds is { Count: >= 2 })
            {
                startX = layout.Bounds[0];
                startY = layout.Bounds[1];
            }
        }

        return new Point2D(
            startX + index % GridColumns * GridStep,
            startY + index / GridColumns * GridStep
        );
    }

    private static void SetIntegerProperty(ItemEntity item, string key, long value)
    {
        item.CustomProperties[key] = new CustomProperty
        {
            Type = CustomPropertyType.Integer,
            IntegerValue = value
        };
    }

    private static bool TryGetIntegerProperty(ItemEntity item, string key, out long value)
    {
        value = 0;

        if (!item.CustomProperties.TryGetValue(key, out var property) || property.Type != CustomPropertyType.Integer)
        {
            return false;
        }

        value = property.IntegerValue;

        return true;
    }

    private static bool TryGetStringProperty(ItemEntity item, string key, out string value)
    {
        value = "";

        if (!item.CustomProperties.TryGetValue(key, out var property) ||
            property.Type != CustomPropertyType.String ||
            string.IsNullOrWhiteSpace(property.StringValue))
        {
            return false;
        }

        value = property.StringValue;

        return true;
    }
}
