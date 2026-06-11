using Moongate.Abstractions.Interfaces.Config;
using Moongate.Abstractions.Types.Secrets;

namespace Moongate.Abstractions.Data.Secrets;

/// <summary>Configures the generic secret manager service.</summary>
public sealed class SecretManagerConfig : IValidatableConfig
{
    /// <summary>Secret provider used to resolve logical secret names.</summary>
    public SecretManagerProviderType Provider { get; set; } = SecretManagerProviderType.Environment;

    /// <summary>Environment-variable provider settings.</summary>
    public EnvironmentSecretManagerConfig Environment { get; set; } = new();

    public IEnumerable<string> Validate()
    {
        if (!Enum.IsDefined(Provider))
        {
            yield return $"Secret provider '{Provider}' is not supported.";
        }
    }
}
