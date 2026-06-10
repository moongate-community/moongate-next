using System.Globalization;
using Moongate.Core.Yaml;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Templates;

/// <summary>
/// Loads item template YAML files from a directory, resolving the
/// <c>base_item</c> inheritance chain and validating the whole set. Any
/// invalid file or template throws so the server fails fast at boot.
/// A missing or empty directory is a warning, not an error.
/// </summary>
/// <remarks>
/// Inheritance uses default-value sentinels: a child field at its default
/// (0, false, empty, Common rarity) inherits the parent value, so a child cannot
/// explicitly re-state a default over a non-default parent value
/// (e.g. <c>hue: 0</c> over a parent's <c>hue: 7</c>, or <c>amount: 1</c>
/// over a parent's <c>amount: 5</c>). Deliberate KISS tradeoff; if explicit
/// overrides become necessary, switch the DTO fields to nullables.
/// </remarks>
public sealed class ItemTemplateYamlLoader
{
    private enum ResolveState : byte
    {
        Unvisited = 0,
        Visiting = 1,
        Done = 2
    }

    private readonly ILogger _logger = Log.ForContext<ItemTemplateYamlLoader>();
    private readonly ITileDataStore? _tileData;
    private readonly string _templatesDirectory;

    public ItemTemplateYamlLoader(string templatesDirectory, ITileDataStore? tileData = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatesDirectory);

        _tileData = tileData;
        _templatesDirectory = Path.GetFullPath(templatesDirectory);
    }

    public List<ItemTemplateDefinition> LoadAll()
    {
        if (!Directory.Exists(_templatesDirectory))
        {
            _logger.Warning(
                "Item templates directory {Directory} not found; no templates loaded",
                _templatesDirectory
            );

            return [];
        }

        var files = Directory.GetFiles(_templatesDirectory, "*.yaml", SearchOption.AllDirectories)
                             .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                             .ToArray();

        if (files.Length == 0)
        {
            _logger.Warning("No item template files found in {Directory}", _templatesDirectory);

            return [];
        }

        var templates = new List<ItemTemplateDefinition>();
        var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            ItemTemplateTable table;

            try
            {
                table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(file);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to parse item template file '{file}'.", exception);
            }

            // YamlDotNet overwrites pre-initialized collections with null when a
            // key is present but empty; normalize so the loader keeps its
            // contextual fail-fast errors instead of raw NullReferenceExceptions.
            foreach (var template in table.ItemTemplates ?? [])
            {
                if (string.IsNullOrWhiteSpace(template.Id))
                {
                    throw new InvalidOperationException($"Item template with empty id in '{file}'.");
                }

                if (sources.TryGetValue(template.Id, out var existingFile))
                {
                    throw new InvalidOperationException(
                        $"Duplicate item template id '{template.Id}' in '{file}' (already defined in '{existingFile}')."
                    );
                }

                // YamlDotNet replaces the pre-initialized dictionary, dropping its
                // case-insensitive comparer; rebuild so param keys stay case-insensitive.
                template.Params = new Dictionary<string, ItemTemplateParamDefinition>(
                    template.Params ?? new Dictionary<string, ItemTemplateParamDefinition>(),
                    StringComparer.OrdinalIgnoreCase
                );

                template.Tags ??= [];

                sources[template.Id] = file;
                templates.Add(template);
            }
        }

        ResolveBaseItems(templates);
        ApplyTileDataNameFallbacks(templates);
        ValidateParams(templates, sources);

        _logger.Information(
            "Loaded {TemplateCount} item templates from {FileCount} files",
            templates.Count,
            files.Length
        );

        return templates;
    }

    internal static long ParseLong(string value)
        => value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
               ? Convert.ToInt64(value[2..], 16)
               : long.Parse(value, CultureInfo.InvariantCulture);

    private static void ApplyInheritance(ItemTemplateDefinition parent, ItemTemplateDefinition child)
    {
        if (string.IsNullOrWhiteSpace(child.Name))
        {
            child.Name = parent.Name;
        }

        if (string.IsNullOrWhiteSpace(child.Comment))
        {
            child.Comment = parent.Comment;
        }

        if (string.IsNullOrWhiteSpace(child.ScriptId))
        {
            child.ScriptId = parent.ScriptId;
        }

        if (child.ItemId == 0)
        {
            child.ItemId = parent.ItemId;
        }

        if (child.Hue == 0)
        {
            child.Hue = parent.Hue;
        }

        if (child.Weight == 0)
        {
            child.Weight = parent.Weight;
        }

        if (child.Amount == 1)
        {
            child.Amount = parent.Amount;
        }

        if (!child.IsStackable)
        {
            child.IsStackable = parent.IsStackable;
        }

        if (!child.IsMovable)
        {
            child.IsMovable = parent.IsMovable;
        }

        child.GumpId ??= parent.GumpId;
        child.Layer ??= parent.Layer;

        if (child.Rarity == ItemRarity.Common)
        {
            child.Rarity = parent.Rarity;
        }

        child.Value ??= parent.Value?.Clone();

        if (child.Visibility == default)
        {
            child.Visibility = parent.Visibility;
        }

        if (child.Tags.Count == 0 && parent.Tags.Count > 0)
        {
            child.Tags = [..parent.Tags];
        }

        child.Params = MergeParams(parent.Params, child.Params);
    }

    private static ItemTemplateParamDefinition CloneParam(ItemTemplateParamDefinition param)
        => new()
        {
            Type = param.Type,
            Value = param.Value
        };

    private static Dictionary<string, ItemTemplateParamDefinition> MergeParams(
        Dictionary<string, ItemTemplateParamDefinition> parentParams,
        Dictionary<string, ItemTemplateParamDefinition> childParams
    )
    {
        var merged = new Dictionary<string, ItemTemplateParamDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, param) in parentParams)
        {
            merged[key] = CloneParam(param);
        }

        foreach (var (key, param) in childParams)
        {
            merged[key] = CloneParam(param);
        }

        return merged;
    }

    private static void ResolveBaseItems(List<ItemTemplateDefinition> templates)
    {
        var byId = templates.ToDictionary(
            static template => template.Id,
            static template => template,
            StringComparer.OrdinalIgnoreCase
        );

        var states = new Dictionary<string, ResolveState>(StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            ResolveTemplate(template, byId, states);
        }
    }

    private static void ResolveTemplate(
        ItemTemplateDefinition template,
        Dictionary<string, ItemTemplateDefinition> byId,
        Dictionary<string, ResolveState> states
    )
    {
        if (states.TryGetValue(template.Id, out var state))
        {
            if (state == ResolveState.Done)
            {
                return;
            }

            if (state == ResolveState.Visiting)
            {
                throw new InvalidOperationException($"Circular base_item reference detected at '{template.Id}'.");
            }
        }

        states[template.Id] = ResolveState.Visiting;

        if (!string.IsNullOrWhiteSpace(template.BaseItem))
        {
            if (!byId.TryGetValue(template.BaseItem, out var parent))
            {
                throw new InvalidOperationException(
                    $"Item template '{template.Id}' references unknown base_item '{template.BaseItem}'."
                );
            }

            ResolveTemplate(parent, byId, states);
            ApplyInheritance(parent, template);
        }

        states[template.Id] = ResolveState.Done;
    }

    private void ApplyTileDataNameFallbacks(List<ItemTemplateDefinition> templates)
    {
        if (_tileData is null)
        {
            return;
        }

        foreach (var template in templates)
        {
            if (!string.IsNullOrWhiteSpace(template.Name) || template.ItemId == 0)
            {
                continue;
            }

            var itemName = _tileData.GetItem(template.ItemId).Name;

            if (!string.IsNullOrWhiteSpace(itemName))
            {
                template.Name = itemName;
            }
        }
    }

    private static void ValidateParams(
        List<ItemTemplateDefinition> templates,
        Dictionary<string, string> sources
    )
    {
        foreach (var template in templates)
        {
            foreach (var (key, param) in template.Params)
            {
                if (string.Equals(key, ItemTemplateDefinition.ReservedIsMovableParamKey, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Item template '{template.Id}' in '{sources[template.Id]}' uses reserved param key '{ItemTemplateDefinition.ReservedIsMovableParamKey}'."
                    );
                }

                if (param.Type == ItemTemplateParamType.String)
                {
                    continue;
                }

                try
                {
                    ParseLong(param.Value);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Item template '{template.Id}' in '{sources[template.Id]}' has invalid {param.Type} param '{key}' = '{param.Value}'.",
                        exception
                    );
                }
            }
        }
    }
}
