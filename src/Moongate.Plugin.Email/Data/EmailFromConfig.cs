namespace Moongate.Plugin.Email.Data;

/// <summary>Sender identity for outgoing email.</summary>
public sealed class EmailFromConfig
{
    /// <summary>Display name used in the From header.</summary>
    public string Name { get; set; } = "Moongate";

    /// <summary>Email address used in the From header.</summary>
    public string Address { get; set; } = "";
}
