namespace Moongate.Plugins.Data;

/// <summary>Sanitized view of a plugin runtime config file.</summary>
public sealed record PluginConfigView(
    string PluginId,
    bool Exists,
    string ConfigPath,
    string SanitizedYaml,
    IReadOnlyList<string> RedactedKeys
);
