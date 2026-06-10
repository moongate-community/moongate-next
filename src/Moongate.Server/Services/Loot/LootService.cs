using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Loot;
using Serilog;
using ShaiRandom.Generators;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Default loot service: walks a loot table tree and produces persisted item
/// entities via the item factory. Stackable items collapse to one entity with
/// the rolled amount; non-stackable items become that many separate entities
/// (capped). The active registry is set at boot.
/// </summary>
public sealed class LootService : ILootService
{
    private const int MaxNonStackableCount = 100;

    private readonly ILogger _logger = Log.ForContext<LootService>();
    private readonly IItemTemplateService _templates;
    private readonly Lazy<IItemFactoryService> _itemFactory;
    private readonly IEnhancedRandom _random;

    private LootTableRegistry? _registry;

    public LootService(
        IItemTemplateService templates,
        Lazy<IItemFactoryService> itemFactory,
        IEnhancedRandom random
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(itemFactory);
        ArgumentNullException.ThrowIfNull(random);

        _templates = templates;
        _itemFactory = itemFactory;
        _random = random;
    }

    public async ValueTask<IReadOnlyList<ItemEntity>> GenerateAsync(
        string lootTableId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lootTableId);

        if (_registry is null || !_registry.TryGet(lootTableId, out var table))
        {
            throw new InvalidOperationException($"Loot table '{lootTableId}' not found.");
        }

        var items = new List<ItemEntity>();

        foreach (var node in table.Content)
        {
            await ResolveNodeAsync(node, items, cancellationToken);
        }

        return items;
    }

    public bool Has(string lootTableId)
        => _registry is not null && _registry.TryGet(lootTableId, out _);

    public void SetRegistry(LootTableRegistry? registry)
        => _registry = registry;

    private async ValueTask CreateItemsAsync(
        string templateId,
        LootAmount? amount,
        List<ItemEntity> sink,
        CancellationToken cancellationToken
    )
    {
        var count = ResolveAmount(amount);

        if (count <= 0)
        {
            return;
        }

        var stackable = _templates.TryGet(templateId, out var template) && template.IsStackable;

        if (stackable)
        {
            sink.Add(await _itemFactory.Value.CreateFromTemplateAsync(templateId, count, cancellationToken));

            return;
        }

        if (count > MaxNonStackableCount)
        {
            _logger.Warning(
                "Loot count {Count} for non-stackable '{Template}' exceeds cap {Cap}; clamping",
                count,
                templateId,
                MaxNonStackableCount
            );

            count = MaxNonStackableCount;
        }

        for (var i = 0; i < count; i++)
        {
            sink.Add(await _itemFactory.Value.CreateFromTemplateAsync(templateId, cancellationToken));
        }
    }

    private LootNode? PickWeighted(List<LootNode> children)
    {
        var total = 0;

        foreach (var child in children)
        {
            total += child.Weight;
        }

        if (total <= 0)
        {
            return null;
        }

        var roll = _random.NextInt(total);

        foreach (var child in children)
        {
            roll -= child.Weight;

            if (roll < 0)
            {
                return child;
            }
        }

        return children[^1];
    }

    private int ResolveAmount(LootAmount? amount)
    {
        if (amount is null)
        {
            return 1;
        }

        return amount.Min == amount.Max ? amount.Min : amount.Min + _random.NextInt(amount.Max - amount.Min + 1);
    }

    private string? ResolveCategory(string tag)
    {
        if (_registry is null || !_registry.TryGetByTag(tag, out var matches) || matches.Count == 0)
        {
            _logger.Warning("Loot category '{Tag}' resolved to no concrete templates; skipping", tag);

            return null;
        }

        return matches[_random.NextInt(matches.Count)].Id;
    }

    private async ValueTask ResolveNodeAsync(LootNode node, List<ItemEntity> sink, CancellationToken cancellationToken)
    {
        if (node.Chance < 1.0 && _random.NextDouble() >= node.Chance)
        {
            return;
        }

        if (node.Group is not null)
        {
            foreach (var child in node.Group)
            {
                await ResolveNodeAsync(child, sink, cancellationToken);
            }

            return;
        }

        if (node.PickOneOf is not null)
        {
            var chosen = PickWeighted(node.PickOneOf);

            if (chosen is not null)
            {
                await ResolveNodeAsync(chosen, sink, cancellationToken);
            }

            return;
        }

        if (node.Item is not null)
        {
            await CreateItemsAsync(node.Item, node.Amount, sink, cancellationToken);

            return;
        }

        if (node.Category is not null)
        {
            var templateId = ResolveCategory(node.Category);

            if (templateId is not null)
            {
                await CreateItemsAsync(templateId, node.Amount, sink, cancellationToken);
            }
        }
    }
}
