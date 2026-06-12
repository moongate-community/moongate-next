using Moongate.Plugins.Configuration;

namespace Moongate.Plugin.Email.Data;

/// <summary>File-system template loading options.</summary>
public sealed class EmailTemplateOptions
{
    /// <summary>Template directory, relative to the plugin directory unless rooted.</summary>
    [ConfigField("Directory", Required = true)]
    public string Directory { get; set; } = "templates";

    /// <summary>Reload templates when source file timestamps change.</summary>
    [ConfigField("Reload on change")]
    public bool ReloadOnChange { get; set; } = true;
}
