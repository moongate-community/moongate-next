using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Boot-time fail-fast validation for loot tables against the item template
/// registry. Any violation throws so the server refuses to start.
/// </summary>
public static class LootTableValidator
{
    public static void Validate(IReadOnlyList<LootTableDefinition> tables, IItemTemplateService templates)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(templates);

        var tagsWithConcreteTemplate = BuildConcreteTagSet(templates);
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var table in tables)
        {
            if (string.IsNullOrWhiteSpace(table.Id))
            {
                throw new InvalidOperationException("Loot table with empty id.");
            }

            if (!seenIds.Add(table.Id))
            {
                throw new InvalidOperationException($"Duplicate loot table id '{table.Id}'.");
            }

            ValidateNodes(table.Id, "content", table.Content, templates, tagsWithConcreteTemplate);
        }
    }

    private static HashSet<string> BuildConcreteTagSet(IItemTemplateService templates)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates.GetAll())
        {
            if (template.IsAbstract)
            {
                continue;
            }

            foreach (var tag in template.Tags)
            {
                tags.Add(tag);
            }
        }

        return tags;
    }

    private static void ValidateAmount(string tableId, string context, LootAmount? amount)
    {
        if (amount is null)
        {
            return;
        }

        if (amount.Min < 0 || amount.Max < 0)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' has negative amount ({amount.Min}..{amount.Max})."
            );
        }

        if (amount.Min > amount.Max)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' has amount min {amount.Min} greater than max {amount.Max}."
            );
        }
    }

    private static void ValidateItemTemplate(
        string tableId,
        string context,
        string templateId,
        IItemTemplateService templates
    )
    {
        if (!templates.TryGet(templateId, out var template))
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' references unknown item template '{templateId}'."
            );
        }

        if (template.IsAbstract)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' references abstract item template '{templateId}'."
            );
        }
    }

    private static void ValidateNode(
        string tableId,
        string context,
        LootNode node,
        IItemTemplateService templates,
        HashSet<string> concreteTags
    )
    {
        var typeCount = (node.Item is not null ? 1 : 0) +
                        (node.Category is not null ? 1 : 0) +
                        (node.PickOneOf is not null ? 1 : 0) +
                        (node.Group is not null ? 1 : 0);

        if (typeCount != 1)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' must have exactly one of item/category/pick_one_of/group (found {typeCount})."
            );
        }

        if (node.Chance is < 0.0 or > 1.0)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' has chance {node.Chance} outside [0, 1]."
            );
        }

        if (node.Weight < 1)
        {
            throw new InvalidOperationException(
                $"Loot table '{tableId}' node '{context}' has weight {node.Weight} below 1."
            );
        }

        ValidateAmount(tableId, context, node.Amount);

        if (node.Item is not null)
        {
            ValidateItemTemplate(tableId, context, node.Item, templates);

            return;
        }

        if (node.Category is not null)
        {
            if (!concreteTags.Contains(node.Category))
            {
                throw new InvalidOperationException(
                    $"Loot table '{tableId}' node '{context}' references category '{node.Category}' with no concrete item template."
                );
            }

            return;
        }

        if (node.PickOneOf is not null)
        {
            if (node.PickOneOf.Count == 0)
            {
                throw new InvalidOperationException($"Loot table '{tableId}' node '{context}' pick_one_of is empty.");
            }

            ValidateNodes(tableId, $"{context}/pick_one_of", node.PickOneOf, templates, concreteTags);

            return;
        }

        if (node.Group!.Count == 0)
        {
            throw new InvalidOperationException($"Loot table '{tableId}' node '{context}' group is empty.");
        }

        ValidateNodes(tableId, $"{context}/group", node.Group, templates, concreteTags);
    }

    private static void ValidateNodes(
        string tableId,
        string context,
        IReadOnlyList<LootNode> nodes,
        IItemTemplateService templates,
        HashSet<string> concreteTags
    )
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            ValidateNode(tableId, $"{context}[{i}]", nodes[i], templates, concreteTags);
        }
    }
}
