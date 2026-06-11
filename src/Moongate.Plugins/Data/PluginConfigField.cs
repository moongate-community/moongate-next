namespace Moongate.Plugins.Data;

/// <summary>Single editable plugin config field descriptor.</summary>
public sealed record PluginConfigField(
    string Path,
    string Label,
    string Type,
    bool Required,
    string? Help,
    IReadOnlyList<string> Options,
    object? Value,
    object? DefaultValue,
    bool SecretReference
);
