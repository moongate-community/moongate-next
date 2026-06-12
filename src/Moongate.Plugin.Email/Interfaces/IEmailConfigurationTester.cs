using Moongate.Plugin.Email.Data;
using Moongate.Plugins.Data;

namespace Moongate.Plugin.Email.Interfaces;

/// <summary>Tests email transport configuration without sending an email.</summary>
public interface IEmailConfigurationTester
{
    /// <summary>Connects and authenticates with the configured SMTP server.</summary>
    ValueTask<PluginTestResult> TestAsync(
        EmailPluginConfig config,
        CancellationToken cancellationToken = default
    );
}
