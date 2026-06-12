using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Services.Templates;

public sealed class ItemTemplateYamlDocumentStore
{
    public const string ManagedFileName = "_web.yaml";

    private readonly string _templatesDirectory;

    public ItemTemplateYamlDocumentStore(string templatesDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatesDirectory);

        _templatesDirectory = Path.GetFullPath(templatesDirectory);
    }

    public string ResolveSourceFile(string templateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        if (Directory.Exists(_templatesDirectory))
        {
            foreach (var file in Directory.GetFiles(_templatesDirectory, "*.yaml", SearchOption.AllDirectories))
            {
                var table = LoadTable(file);

                if (table.ItemTemplates.Any(template => string.Equals(
                        template.Id,
                        templateId,
                        StringComparison.OrdinalIgnoreCase
                    )))
                {
                    return file;
                }
            }
        }

        return Path.Combine(_templatesDirectory, ManagedFileName);
    }

    public ItemTemplateTable LoadTable(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalized = NormalizeInsideTemplatesDirectory(filePath);

        if (!File.Exists(normalized))
        {
            return new() { ItemTemplates = [] };
        }

        var table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(normalized);
        table.ItemTemplates ??= [];

        return table;
    }

    public void Upsert(string filePath, ItemTemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var normalized = NormalizeInsideTemplatesDirectory(filePath);
        var table = LoadTable(normalized);
        var existingIndex = table.ItemTemplates.FindIndex(
            existing => string.Equals(existing.Id, template.Id, StringComparison.OrdinalIgnoreCase)
        );

        if (existingIndex >= 0)
        {
            table.ItemTemplates[existingIndex] = template;
        }
        else
        {
            table.ItemTemplates.Add(template);
        }

        WriteAtomically(table, normalized);
    }

    private string NormalizeInsideTemplatesDirectory(string filePath)
    {
        var normalized = Path.GetFullPath(filePath);
        var relativePath = Path.GetRelativePath(_templatesDirectory, normalized);

        if (Path.IsPathRooted(relativePath) || relativePath.StartsWith("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Template file '{normalized}' is outside '{_templatesDirectory}'.");
        }

        return normalized;
    }

    private static void WriteAtomically(ItemTemplateTable table, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(tempPath, YamlUtils.Serialize(table));
            File.Move(tempPath, filePath, true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
