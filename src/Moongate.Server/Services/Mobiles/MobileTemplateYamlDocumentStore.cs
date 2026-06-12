using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Server.Services.Mobiles;

public sealed class MobileTemplateYamlDocumentStore
{
    public const string ManagedFileName = "_web.yaml";

    private readonly string _templatesDirectory;

    public MobileTemplateYamlDocumentStore(string templatesDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatesDirectory);

        _templatesDirectory = Path.GetFullPath(templatesDirectory);
    }

    public MobileTemplateTable LoadTable(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalized = NormalizeInsideTemplatesDirectory(filePath);

        if (!File.Exists(normalized))
        {
            return new() { MobileTemplates = [] };
        }

        var table = YamlUtils.DeserializeFromFile<MobileTemplateTable>(normalized);
        table.MobileTemplates ??= [];

        return table;
    }

    public string ResolveSourceFile(string templateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        if (Directory.Exists(_templatesDirectory))
        {
            foreach (var file in Directory.GetFiles(_templatesDirectory, "*.yaml", SearchOption.AllDirectories))
            {
                var table = LoadTable(file);

                if (table.MobileTemplates.Any(
                        template => string.Equals(
                            template.Id,
                            templateId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ))
                {
                    return file;
                }
            }
        }

        return Path.Combine(_templatesDirectory, ManagedFileName);
    }

    public void Upsert(string filePath, MobileTemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var normalized = NormalizeInsideTemplatesDirectory(filePath);
        var table = LoadTable(normalized);
        var existingIndex = table.MobileTemplates.FindIndex(
            existing => string.Equals(existing.Id, template.Id, StringComparison.OrdinalIgnoreCase)
        );

        if (existingIndex >= 0)
        {
            table.MobileTemplates[existingIndex] = template;
        }
        else
        {
            table.MobileTemplates.Add(template);
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

    private static void WriteAtomically(MobileTemplateTable table, string filePath)
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
