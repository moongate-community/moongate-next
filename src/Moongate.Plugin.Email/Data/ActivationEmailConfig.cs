using Moongate.Plugins.Configuration;
using Moongate.Plugins.Types;

namespace Moongate.Plugin.Email.Data;

/// <summary>Activation email behavior.</summary>
public sealed class ActivationEmailConfig
{
    /// <summary>Template id under the configured template directory.</summary>
    [ConfigField("Template id", Required = true)]
    public string TemplateId { get; set; } = "account_activation";

    /// <summary>Activation URL template. Must contain <c>{activation_id}</c>.</summary>
    [ConfigField(
        "Activation URL template",
        Type = ConfigFieldType.TextArea,
        Required = true,
        Help = "Must contain {activation_id}."
    )]
    public string UrlTemplate { get; set; } = "https://example.com/activate?activation_id={activation_id}";
}
