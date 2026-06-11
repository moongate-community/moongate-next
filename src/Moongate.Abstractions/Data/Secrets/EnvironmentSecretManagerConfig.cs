namespace Moongate.Abstractions.Data.Secrets;

/// <summary>Environment-variable secret provider settings.</summary>
public sealed class EnvironmentSecretManagerConfig
{
    /// <summary>Optional prefix prepended to normalized secret names.</summary>
    public string Prefix { get; set; } = "";
}
