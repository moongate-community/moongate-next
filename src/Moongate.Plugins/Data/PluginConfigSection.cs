namespace Moongate.Plugins.Data;

/// <summary>Group of related plugin config fields.</summary>
public sealed record PluginConfigSection(
    string Id,
    string Label,
    IReadOnlyList<PluginConfigField> Fields
);
