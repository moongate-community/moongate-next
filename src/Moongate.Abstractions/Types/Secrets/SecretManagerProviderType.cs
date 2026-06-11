namespace Moongate.Abstractions.Types.Secrets;

/// <summary>Supported secret manager backends.</summary>
public enum SecretManagerProviderType
{
    /// <summary>Resolve secrets from process environment variables.</summary>
    Environment
}
