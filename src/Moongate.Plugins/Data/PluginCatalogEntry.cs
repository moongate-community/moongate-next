namespace Moongate.Plugins.Data;

/// <summary>Sanitized metadata for a loaded Moongate plugin.</summary>
public sealed record PluginCatalogEntry(
    string Id,
    string Name,
    string Version,
    string Author,
    string? Description,
    IReadOnlyList<string> Dependencies,
    string AssemblyName,
    string DirectoryName,
    bool HasConfig
);
