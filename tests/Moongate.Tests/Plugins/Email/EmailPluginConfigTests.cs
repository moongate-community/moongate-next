using Moongate.Abstractions.Types.Secrets;
using Moongate.Plugin.Email.Data;

namespace Moongate.Tests.Plugins.Email;

public sealed class EmailPluginConfigTests
{
    [Fact]
    public void Validate_DisabledDefault_ReturnsNoErrors()
    {
        var config = new EmailPluginConfig();

        Assert.Empty(config.Validate());
    }

    [Fact]
    public void Validate_EnabledMissingRequiredValues_ReturnsErrors()
    {
        var config = new EmailPluginConfig
        {
            Enabled = true
        };

        var errors = config.Validate().ToArray();

        Assert.Contains(errors, error => error.Contains("From.Address", StringComparison.Ordinal));
        Assert.Contains(errors, error => error.Contains("Smtp.Host", StringComparison.Ordinal));
        Assert.Contains(errors, error => error.Contains("Smtp.Username", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_EnabledInvalidSecretProvider_ReturnsError()
    {
        var config = ValidConfig();
        config.Secrets.Provider = (SecretManagerProviderType)99;

        var errors = config.Validate().ToArray();

        Assert.Contains(errors, error => error.Contains("Secrets.Secret provider", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_EnabledValidConfig_ReturnsNoErrors()
    {
        var config = ValidConfig();

        Assert.Empty(config.Validate());
    }

    private static EmailPluginConfig ValidConfig()
    {
        var config = new EmailPluginConfig
        {
            Enabled = true,
            From =
            {
                Address = "noreply@example.com"
            },
            Smtp =
            {
                Host = "smtp.example.com",
                Username = "noreply@example.com",
                PasswordSecret = "smtp_password"
            },
            Activation =
            {
                UrlTemplate = "https://example.com/activate?activation_id={activation_id}"
            }
        };

        return config;
    }
}
