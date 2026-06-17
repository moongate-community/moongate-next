using MailKit.Net.Smtp;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.Plugins.Data;

namespace Moongate.Plugin.Email.Services;

/// <summary>SMTP connection/authentication tester.</summary>
public sealed class SmtpEmailConfigurationTester : IEmailConfigurationTester
{
    private readonly ISecretManagerService _secrets;

    public SmtpEmailConfigurationTester(ISecretManagerService secrets)
    {
        _secrets = secrets;
    }

    public async ValueTask<PluginTestResult> TestAsync(
        EmailPluginConfig config,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(config);

        var password = await _secrets.GetSecretAsync(config.Smtp.PasswordSecret, cancellationToken);

        if (string.IsNullOrEmpty(password))
        {
            return new PluginTestResult(
                false,
                "SMTP secret was not found.",
                [$"Missing secret: {config.Smtp.PasswordSecret}"]
            );
        }

        try
        {
            using var smtp = new SmtpClient();
            smtp.Timeout = config.Smtp.TimeoutSeconds * 1000;

            await smtp.ConnectAsync(
                config.Smtp.Host,
                config.Smtp.Port,
                SmtpSecurityOptions.Resolve(config.Smtp),
                cancellationToken
            );
            await smtp.AuthenticateAsync(config.Smtp.Username, password, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            return new PluginTestResult(true, "SMTP connection and authentication succeeded.", []);
        }
        catch (Exception ex)
        {
            return new PluginTestResult(false, "SMTP connection or authentication failed.", [ex.Message]);
        }
    }
}
