namespace Moongate.Plugins.Data;

/// <summary>Result returned after saving plugin configuration.</summary>
public sealed record PluginConfigSaveResult(
    bool Success,
    bool RequiresRestart,
    IReadOnlyList<string> Errors,
    PluginConfigView? Config
);
