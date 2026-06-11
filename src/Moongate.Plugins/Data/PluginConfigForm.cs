namespace Moongate.Plugins.Data;

/// <summary>Small plugin config form descriptor rendered by the admin UI.</summary>
public sealed record PluginConfigForm(
    IReadOnlyList<PluginConfigSection> Sections
);
