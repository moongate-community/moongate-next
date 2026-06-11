namespace Moongate.Plugins.Data;

/// <summary>Flat plugin config values keyed by dotted field path.</summary>
public sealed record PluginConfigSaveRequest(
    Dictionary<string, object?> Values
);
