using System.Globalization;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Server.Services.Loot;

public sealed class LootTemplateProjectionService
{
    private readonly IReadOnlyList<ItemTemplateDefinition> _templates;
    private readonly Dictionary<string, ItemTemplateDefinition> _byId;
    private readonly Dictionary<string, IReadOnlyList<ItemTemplateDefinition>> _byTag;

    public LootTemplateProjectionService(IEnumerable<ItemTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        _templates = templates.ToArray();
        _byId = _templates.ToDictionary(static template => template.Id, StringComparer.OrdinalIgnoreCase);
        _byTag = _templates
                 .Where(static template => !template.IsAbstract)
                 .SelectMany(static template => template.Tags.Select(tag => (Tag: tag, Template: template)))
                 .GroupBy(static pair => pair.Tag, StringComparer.OrdinalIgnoreCase)
                 .ToDictionary(
                     static group => group.Key,
                     static group => (IReadOnlyList<ItemTemplateDefinition>)group
                         .Select(static pair => pair.Template)
                         .OrderBy(static template => template.Id, StringComparer.OrdinalIgnoreCase)
                         .ToArray(),
                     StringComparer.OrdinalIgnoreCase
                 );
    }

    public LootTemplateDetail Project(LootTableDefinition table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var rows = new List<LootTemplateNodeSummary>();
        var potentialItems = new List<LootTemplateNodeSummary>();

        for (var i = 0; i < table.Content.Count; i++)
        {
            AddNode(rows, potentialItems, table.Content[i], "", 0, i);
        }

        var previewItems = potentialItems
                           .Where(static row => row.ItemTemplateId is not null)
                           .GroupBy(static row => row.ItemTemplateId, StringComparer.OrdinalIgnoreCase)
                           .Select(static group => group.First())
                           .Take(24)
                           .ToArray();

        return new(table.Id, table.Content.Count, rows, potentialItems, previewItems);
    }

    private static string FormatItemId(int itemId)
        => $"0x{itemId.ToString("X4", CultureInfo.InvariantCulture)}";

    private static string FormatItemImageUrl(int itemId)
        => $"/api/items/{FormatItemId(itemId)}.png";

    private void AddCategoryChildren(
        List<LootTemplateNodeSummary> potentialItems,
        LootNode node,
        string parentId,
        int depth
    )
    {
        if (node.Category is null || !_byTag.TryGetValue(node.Category, out var matches))
        {
            return;
        }

        var candidateChance = node.Chance / matches.Count;

        for (var i = 0; i < matches.Count; i++)
        {
            var template = matches[i];
            var id = $"{parentId}.candidate.{i.ToString(CultureInfo.InvariantCulture)}";

            potentialItems.Add(CreateItemRow(id, parentId, depth, "category_candidate", template, node.Amount, candidateChance));
        }
    }

    private void AddNode(
        List<LootTemplateNodeSummary> rows,
        List<LootTemplateNodeSummary> potentialItems,
        LootNode node,
        string parentId,
        int depth,
        int index
    )
    {
        var id = string.IsNullOrWhiteSpace(parentId)
                     ? index.ToString(CultureInfo.InvariantCulture)
                     : $"{parentId}.{index.ToString(CultureInfo.InvariantCulture)}";
        var kind = ResolveKind(node);
        var row = CreateRow(id, parentId, depth, kind, node);
        rows.Add(row);

        if (row.ItemTemplateId is not null)
        {
            potentialItems.Add(row);
        }

        if (node.Group is not null)
        {
            for (var i = 0; i < node.Group.Count; i++)
            {
                AddNode(rows, potentialItems, node.Group[i], id, depth + 1, i);
            }
        }

        if (node.PickOneOf is not null)
        {
            for (var i = 0; i < node.PickOneOf.Count; i++)
            {
                AddNode(rows, potentialItems, node.PickOneOf[i], id, depth + 1, i);
            }
        }

        if (node.Category is not null)
        {
            AddCategoryChildren(potentialItems, node, id, depth + 1);
        }
    }

    private LootTemplateNodeSummary CreateRow(string id, string parentId, int depth, string kind, LootNode node)
    {
        var amountMin = node.Amount?.Min ?? 1;
        var amountMax = node.Amount?.Max ?? 1;
        var label = node.Item ?? node.Category ?? kind;
        string? rarity = null;
        string? itemIdHex = null;
        string? imageUrl = null;
        var stackable = false;

        if (node.Item is not null && _byId.TryGetValue(node.Item, out var template))
        {
            label = string.IsNullOrWhiteSpace(template.Name) ? template.Id : template.Name;
            rarity = template.Rarity.ToString();
            itemIdHex = FormatItemId(template.ItemId);
            imageUrl = FormatItemImageUrl(template.ItemId);
            stackable = template.IsStackable;
        }

        return new(
            id,
            parentId,
            depth,
            kind,
            label,
            rarity,
            node.Chance,
            node.Weight,
            amountMin,
            amountMax,
            node.Item,
            itemIdHex,
            imageUrl,
            stackable
        );
    }

    private static LootTemplateNodeSummary CreateItemRow(
        string id,
        string parentId,
        int depth,
        string kind,
        ItemTemplateDefinition template,
        LootAmount? amount,
        double chance
    )
    {
        var amountMin = amount?.Min ?? 1;
        var amountMax = amount?.Max ?? 1;
        var label = string.IsNullOrWhiteSpace(template.Name) ? template.Id : template.Name;
        var itemIdHex = FormatItemId(template.ItemId);

        return new(
            id,
            parentId,
            depth,
            kind,
            label,
            template.Rarity.ToString(),
            chance,
            0,
            amountMin,
            amountMax,
            template.Id,
            itemIdHex,
            FormatItemImageUrl(template.ItemId),
            template.IsStackable
        );
    }

    private static string ResolveKind(LootNode node)
    {
        if (node.Item is not null)
        {
            return "item";
        }

        if (node.Category is not null)
        {
            return "category";
        }

        if (node.PickOneOf is not null)
        {
            return "pick_one_of";
        }

        return "group";
    }
}
