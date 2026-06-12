using Moongate.Plugins.Configuration;

namespace Moongate.Plugin.Email.Data;

/// <summary>Sender identity for outgoing email.</summary>
public sealed class EmailFromConfig
{
    /// <summary>Display name used in the From header.</summary>
    [ConfigField("From name")]
    public string Name { get; set; } = "Moongate";

    /// <summary>Email address used in the From header.</summary>
    [ConfigField("From address", Required = true)]
    public string Address { get; set; } = "";
}
