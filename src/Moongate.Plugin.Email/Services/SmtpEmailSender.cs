using MailKit.Net.Smtp;
using MimeKit;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;

namespace Moongate.Plugin.Email.Services;

/// <summary>MailKit SMTP email sender.</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailPluginConfig _config;
    private readonly ISecretManagerService _secrets;

    public SmtpEmailSender(EmailPluginConfig config, ISecretManagerService secrets)
    {
        _config = config;
        _secrets = secrets;
    }

    public async ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var password = await _secrets.GetSecretAsync(_config.Smtp.PasswordSecret, cancellationToken);

        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException($"SMTP secret '{_config.Smtp.PasswordSecret}' was not found.");
        }

        using var smtp = new SmtpClient();
        smtp.Timeout = _config.Smtp.TimeoutSeconds * 1000;

        await smtp.ConnectAsync(
            _config.Smtp.Host,
            _config.Smtp.Port,
            SmtpSecurityOptions.Resolve(_config.Smtp),
            cancellationToken
        );
        await smtp.AuthenticateAsync(_config.Smtp.Username, password, cancellationToken);
        await smtp.SendAsync(BuildMessage(message), cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }

    private MimeMessage BuildMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_config.From.Name, _config.From.Address));
        mime.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mime.Subject = message.Subject;

        var body = new BodyBuilder
        {
            TextBody = message.TextBody
        };

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            body.HtmlBody = message.HtmlBody;
        }

        mime.Body = body.ToMessageBody();

        return mime;
    }
}
