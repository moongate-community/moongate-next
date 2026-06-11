namespace Moongate.Plugins.Data;

/// <summary>Result returned by an optional plugin runtime configuration test.</summary>
public sealed record PluginTestResult(
    bool Success,
    string Message,
    IReadOnlyList<string> Details
);
