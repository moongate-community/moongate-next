using Moongate.Abstractions.Data.Internal;
using Moongate.Abstractions.Interfaces.Config;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Abstractions.Configuration;

/// <summary>
///     Loads the single YAML config file once at boot: creates it with defaults when missing, binds each
///     registered section, validates, and fails fast on malformed YAML or invalid values. Stateless.
/// </summary>
public static class ConfigService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(ConfigService));

    /// <summary>
    ///     Loads (and self-heals) the config file for the given section registrations.
    /// </summary>
    /// <param name="filePath">Full path to the YAML file.</param>
    /// <param name="sections">Registered config sections.</param>
    /// <returns>One bound instance per section, in registration order.</returns>
    public static IReadOnlyList<ConfigLoadResult> Load(
        string filePath,
        IReadOnlyList<ConfigSectionRegistration> sections
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(sections);

        var fullPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileExisted = File.Exists(fullPath);
        var root = fileExisted ? ParseRoot(fullPath) : new Dictionary<string, object?>();

        var results = new List<ConfigLoadResult>(sections.Count);
        var errors = new List<string>();
        var dirty = !fileExisted;

        foreach (var section in sections)
        {
            object instance;

            if (root.TryGetValue(section.Name, out var raw) && raw is not null)
            {
                instance = BindSection(section, raw);
            }
            else
            {
                instance = section.CreateDefault();
                dirty = true;
            }

            if (instance is IValidatableConfig validatable)
            {
                errors.AddRange(validatable.Validate().Select(e => $"[{section.Name}] {e}"));
            }

            results.Add(new ConfigLoadResult(section.Type, instance));
        }

        WarnUnknownSections(root, sections);

        if (errors.Count > 0)
        {
            var message = "Invalid configuration:\n" + string.Join("\n", errors);
            _logger.Fatal("{Message}", message);

            throw new InvalidOperationException(message);
        }

        if (dirty)
        {
            WriteFile(fullPath, results, sections);
            _logger.Information(
                fileExisted ? "Config healed at {Path}" : "Created default config at {Path}",
                fullPath
            );
        }

        return results;
    }

    private static object BindSection(ConfigSectionRegistration section, object raw)
    {
        try
        {
            var sectionYaml = ConfigYamlOptions.Serializer.Serialize(raw);

            return ConfigYamlOptions.Deserializer.Deserialize(sectionYaml, section.Type) ??
                   throw new InvalidOperationException($"Config section [{section.Name}] returned null.");
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Config section [{Section}] is invalid", section.Name);

            throw new InvalidOperationException($"Config section [{section.Name}] could not be parsed.", ex);
        }
    }

    private static Dictionary<string, object?> ParseRoot(string fullPath)
    {
        var text = File.ReadAllText(fullPath);

        try
        {
            return ConfigYamlOptions.Deserializer.Deserialize<Dictionary<string, object?>>(text) ?? [];
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Malformed config {Path}", fullPath);

            throw new InvalidOperationException($"Malformed config file '{fullPath}'.", ex);
        }
    }

    private static void WarnUnknownSections(
        Dictionary<string, object?> root,
        IReadOnlyList<ConfigSectionRegistration> sections
    )
    {
        var known = sections.Select(s => s.Name).ToHashSet();

        foreach (var key in root.Keys)
        {
            if (!known.Contains(key))
            {
                _logger.Warning("Ignoring unknown config section [{Section}]", key);
            }
        }
    }

    private static void WriteFile(
        string fullPath,
        IReadOnlyList<ConfigLoadResult> results,
        IReadOnlyList<ConfigSectionRegistration> sections
    )
    {
        var root = new Dictionary<string, object?>(StringComparer.Ordinal);

        for (var i = 0; i < results.Count; i++)
        {
            root[sections[i].Name] = results[i].Instance;
        }

        var tempPath = fullPath + ".tmp";
        File.WriteAllText(tempPath, ConfigYamlOptions.Serializer.Serialize(root));
        File.Move(tempPath, fullPath, true);
    }
}
