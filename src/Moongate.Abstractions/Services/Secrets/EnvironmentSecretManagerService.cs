using System.Text;
using Moongate.Abstractions.Data.Secrets;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Abstractions.Services.Secrets;

/// <summary>Secret manager backed by process environment variables.</summary>
public sealed class EnvironmentSecretManagerService : ISecretManagerService
{
    private readonly EnvironmentSecretManagerConfig _config;
    private readonly Func<string, string?> _getEnvironmentVariable;

    public EnvironmentSecretManagerService(SecretManagerConfig config)
        : this(config.Environment) { }

    public EnvironmentSecretManagerService(EnvironmentSecretManagerConfig config)
        : this(config, Environment.GetEnvironmentVariable) { }

    internal EnvironmentSecretManagerService(
        EnvironmentSecretManagerConfig config,
        Func<string, string?> getEnvironmentVariable
    )
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(getEnvironmentVariable);

        _config = config;
        _getEnvironmentVariable = getEnvironmentVariable;
    }

    public ValueTask<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        if (string.IsNullOrWhiteSpace(name))
        {
            return ValueTask.FromResult<string?>(null);
        }

        var environmentName = ResolveEnvironmentVariableName(name);

        return ValueTask.FromResult(_getEnvironmentVariable(environmentName));
    }

    internal string ResolveEnvironmentVariableName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _config.Prefix + NormalizeSecretName(name);
    }

    private static string NormalizeSecretName(string name)
    {
        var builder = new StringBuilder(name.Length);
        var previousWasSeparator = false;

        foreach (var c in name.Trim())
        {
            if (char.IsAsciiLetterOrDigit(c))
            {
                builder.Append(char.ToUpperInvariant(c));
                previousWasSeparator = false;

                continue;
            }

            if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('_');
                previousWasSeparator = true;
            }
        }

        if (builder.Length > 0 && builder[^1] == '_')
        {
            builder.Length--;
        }

        return builder.ToString();
    }
}
