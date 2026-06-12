using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Services.Templates;

public sealed class ItemTemplateAuthoringService : IItemTemplateAuthoringService
{
    private readonly ItemTemplateYamlDocumentStore _documents;
    private readonly IHueStore _hues;
    private readonly IItemService _items;
    private readonly string _itemsDirectory;
    private readonly ILootService _lootService;
    private readonly LootTableRegistryStore _lootRegistryStore;
    private readonly object _saveGate = new();
    private readonly IItemTemplateService _templates;
    private readonly ITileDataStore _tileData;

    public ItemTemplateAuthoringService(
        DirectoriesConfig directories,
        ITileDataStore tileData,
        IItemTemplateService templates,
        IHueStore hues,
        LootTableRegistryStore lootRegistryStore,
        IItemService items,
        ILootService lootService
    )
    {
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(tileData);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(hues);
        ArgumentNullException.ThrowIfNull(lootRegistryStore);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(lootService);

        _itemsDirectory = directories[DirectoryType.Templates_Items];
        _documents = new(_itemsDirectory);
        _tileData = tileData;
        _templates = templates;
        _hues = hues;
        _lootRegistryStore = lootRegistryStore;
        _items = items;
        _lootService = lootService;
    }

    public ValueTask<ItemTemplateSaveResult> CreateAsync(
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var template = ToDefinition(request);

        lock (_saveGate)
        {
            if (TemplateExists(template.Id))
            {
                throw new InvalidOperationException($"Item template '{template.Id}' already exists.");
            }

            var sourceFile = Path.Combine(_itemsDirectory, ItemTemplateYamlDocumentStore.ManagedFileName);

            return ValueTask.FromResult(Save(template, sourceFile, cancellationToken));
        }
    }

    public ValueTask<ItemTemplateSaveResult?> UpdateAsync(
        string id,
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var template = ToDefinition(request);

        if (!string.Equals(id.Trim(), template.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Route id must match the request id.");
        }

        lock (_saveGate)
        {
            if (!_templates.TryGet(id, out var existingTemplate))
            {
                return ValueTask.FromResult<ItemTemplateSaveResult?>(null);
            }

            if (!string.IsNullOrWhiteSpace(existingTemplate.BaseItem))
            {
                throw new InvalidOperationException("base_item templates cannot be authored from the web editor yet.");
            }

            var sourceFile = _documents.ResolveSourceFile(template.Id);

            return ValueTask.FromResult<ItemTemplateSaveResult?>(Save(template, sourceFile, cancellationToken));
        }
    }

    private ItemTemplateSaveResult Save(
        ItemTemplateDefinition template,
        string sourceFile,
        CancellationToken cancellationToken
    )
    {
        Validate(template);
        cancellationToken.ThrowIfCancellationRequested();

        var relativeSourceFile = RelativeSourceFile(sourceFile);
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "moongate-item-template-authoring",
            Guid.NewGuid().ToString("N")
        );

        try
        {
            CopyDirectory(_itemsDirectory, tempDirectory);
            var tempSourceFile = Path.Combine(tempDirectory, relativeSourceFile);
            var tempDocuments = new ItemTemplateYamlDocumentStore(tempDirectory);
            tempDocuments.Upsert(tempSourceFile, template);

            cancellationToken.ThrowIfCancellationRequested();
            var candidateTemplates = new ItemTemplateYamlLoader(tempDirectory, _tileData).LoadAll();
            ValidateCandidateContents(candidateTemplates);

            cancellationToken.ThrowIfCancellationRequested();
            _documents.Upsert(sourceFile, template);

            var reloaded = new ItemTemplateYamlLoader(_itemsDirectory, _tileData).LoadAll();
            _templates.ReplaceAll(reloaded);
            PublishLootRegistry(reloaded);

            var savedTemplate = reloaded.Single(
                item => string.Equals(item.Id, template.Id, StringComparison.OrdinalIgnoreCase)
            );

            return new(ItemTemplateDetail.FromDefinition(savedTemplate, _hues), relativeSourceFile);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    private void PublishLootRegistry(IReadOnlyList<ItemTemplateDefinition> templates)
    {
        var registry = new LootTableRegistry(_lootRegistryStore.Registry.GetAll(), templates);
        _lootRegistryStore.SetRegistry(registry);

        if (_lootService is LootService lootService)
        {
            lootService.SetRegistry(registry);
        }
    }

    private void ValidateCandidateContents(IReadOnlyList<ItemTemplateDefinition> templates)
    {
        var lootRegistry = _lootRegistryStore.Registry;
        var candidateTemplateService = new ItemTemplateService();
        candidateTemplateService.ReplaceAll(templates);
        LootTableValidator.Validate(lootRegistry.GetAll(), candidateTemplateService);

        if (!templates.Any(static template => template.Contents is not null))
        {
            return;
        }

        var candidateRegistry = new LootTableRegistry(lootRegistry.GetAll(), templates);
        ItemTemplateContentsValidator.Validate(templates, candidateRegistry, _items);
    }

    private bool TemplateExists(string id)
    {
        if (_templates.TryGet(id, out _))
        {
            return true;
        }

        var sourceFile = _documents.ResolveSourceFile(id);

        return File.Exists(sourceFile) &&
               _documents.LoadTable(sourceFile)
                         .ItemTemplates
                         .Any(template => string.Equals(template.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private static ItemTemplateDefinition ToDefinition(ItemTemplateEditRequest request)
    {
        var id = request.Id.Trim();
        var tags = (request.Tags ?? [])
                   .Select(static tag => tag.Trim())
                   .Where(static tag => tag.Length > 0)
                   .Distinct(StringComparer.OrdinalIgnoreCase)
                   .ToList();
        var parameters = new Dictionary<string, ItemTemplateParamDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, param) in request.Params ?? new(StringComparer.OrdinalIgnoreCase))
        {
            parameters[key.Trim()] = new()
            {
                Type = param.Type,
                Value = param.Value
            };
        }

        return new()
        {
            Id = id,
            BaseItem = string.IsNullOrWhiteSpace(request.BaseItem) ? null : request.BaseItem.Trim(),
            IsAbstract = request.IsAbstract,
            Name = request.Name.Trim(),
            Comment = request.Comment.Trim(),
            ItemId = request.ItemId,
            GraphicVariants = [.. (request.GraphicVariants ?? []).Select(CloneGraphicVariant)],
            Hue = request.Hue,
            Weight = request.Weight,
            Amount = request.Amount,
            IsStackable = request.IsStackable,
            IsMovable = request.IsMovable,
            GumpId = request.GumpId,
            Layer = request.Layer,
            ScriptId = request.ScriptId.Trim(),
            Rarity = request.Rarity,
            Value = request.Value?.Clone(),
            Contents = NormalizeContents(request.Contents),
            Visibility = request.Visibility,
            Tags = tags,
            Params = parameters
        };
    }

    private static ItemTemplateGraphicVariantDefinition CloneGraphicVariant(ItemTemplateGraphicVariantDefinition variant)
        => new() { ItemId = variant.ItemId };

    private static ItemTemplateContentsDefinition? NormalizeContents(ItemTemplateContentsDefinition? contents)
    {
        if (contents is null || string.IsNullOrWhiteSpace(contents.LootTemplate))
        {
            return null;
        }

        return new()
        {
            LootTemplate = contents.LootTemplate.Trim(),
            Generate = contents.Generate,
            RefillEvery = contents.RefillEvery,
            RefillPolicy = contents.RefillPolicy,
            RefillScope = contents.RefillScope
        };
    }

    private static void Validate(ItemTemplateDefinition template)
    {
        if (!string.IsNullOrWhiteSpace(template.BaseItem))
        {
            throw new InvalidOperationException("base_item templates cannot be authored from the web editor yet.");
        }

        if (string.IsNullOrWhiteSpace(template.Id))
        {
            throw new InvalidOperationException("Item template id is required.");
        }

        if (template.ItemId < 0)
        {
            throw new InvalidOperationException("item_id must be greater than or equal to 0.");
        }

        if (template.Amount < 1)
        {
            throw new InvalidOperationException("amount must be greater than or equal to 1.");
        }

        if (template.Weight < 0)
        {
            throw new InvalidOperationException("weight must be greater than or equal to 0.");
        }

        if (template.Hue < 0)
        {
            throw new InvalidOperationException("hue must be greater than or equal to 0.");
        }

        if (template.GumpId < 0)
        {
            throw new InvalidOperationException("gump_id must be greater than or equal to 0.");
        }

        ValidateGraphicVariants(template);
        ValidateParams(template);
    }

    private static void ValidateGraphicVariants(ItemTemplateDefinition template)
    {
        var itemIds = new HashSet<int>();

        foreach (var variant in template.GraphicVariants)
        {
            if (variant.ItemId <= 0)
            {
                throw new InvalidOperationException("graphic_variants item_id must be greater than 0.");
            }

            if (variant.ItemId == template.ItemId)
            {
                throw new InvalidOperationException("graphic_variants item_id must not match item_id.");
            }

            if (!itemIds.Add(variant.ItemId))
            {
                throw new InvalidOperationException("graphic_variants item_id values must be unique.");
            }
        }
    }

    private static void ValidateParams(ItemTemplateDefinition template)
    {
        foreach (var (key, _) in template.Params)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("params keys must not be blank.");
            }
        }
    }

    private string RelativeSourceFile(string sourceFile)
        => Path.GetRelativePath(_itemsDirectory, sourceFile).Replace(Path.DirectorySeparatorChar, '/');

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, directory)));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, file));
            var targetParent = Path.GetDirectoryName(targetFile);

            if (!string.IsNullOrWhiteSpace(targetParent))
            {
                Directory.CreateDirectory(targetParent);
            }

            File.Copy(file, targetFile, true);
        }
    }
}
