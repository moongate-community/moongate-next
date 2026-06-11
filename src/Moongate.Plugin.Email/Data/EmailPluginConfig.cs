using System.Net.Mail;
using Moongate.Abstractions.Data.Secrets;
using Moongate.Abstractions.Interfaces.Config;

namespace Moongate.Plugin.Email.Data;

/// <summary>Runtime config for the Moongate email plugin.</summary>
public sealed class EmailPluginConfig : IValidatableConfig
{
    /// <summary>Whether the plugin should send email.</summary>
    public bool Enabled { get; set; }

    /// <summary>Sender identity.</summary>
    public EmailFromConfig From { get; set; } = new();

    /// <summary>SMTP transport settings.</summary>
    public SmtpConfig Smtp { get; set; } = new();

    /// <summary>Secret resolution settings.</summary>
    public SecretManagerConfig Secrets { get; set; } = new()
    {
        Environment =
        {
            Prefix = "MOONGATE_EMAIL_"
        }
    };

    /// <summary>Activation email settings.</summary>
    public ActivationEmailConfig Activation { get; set; } = new();

    /// <summary>Template loading settings.</summary>
    public EmailTemplateOptions Templates { get; set; } = new();

    public IEnumerable<string> Validate()
    {
        if (!Enabled)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(From.Address) || !MailAddress.TryCreate(From.Address, out _))
        {
            yield return "From.Address must be a valid email address.";
        }

        if (string.IsNullOrWhiteSpace(Smtp.Host))
        {
            yield return "Smtp.Host is required when email is enabled.";
        }

        if (Smtp.Port <= 0)
        {
            yield return "Smtp.Port must be greater than zero.";
        }

        if (Smtp.TimeoutSeconds <= 0)
        {
            yield return "Smtp.TimeoutSeconds must be greater than zero.";
        }

        if (string.IsNullOrWhiteSpace(Smtp.Username))
        {
            yield return "Smtp.Username is required when email is enabled.";
        }

        if (string.IsNullOrWhiteSpace(Smtp.PasswordSecret))
        {
            yield return "Smtp.PasswordSecret is required when email is enabled.";
        }

        if (string.IsNullOrWhiteSpace(Activation.TemplateId))
        {
            yield return "Activation.TemplateId is required when email is enabled.";
        }

        if (string.IsNullOrWhiteSpace(Activation.UrlTemplate) ||
            !Activation.UrlTemplate.Contains("{activation_id}", StringComparison.Ordinal))
        {
            yield return "Activation.UrlTemplate must contain '{activation_id}'.";
        }

        if (string.IsNullOrWhiteSpace(Templates.Directory))
        {
            yield return "Templates.Directory is required when email is enabled.";
        }

        foreach (var error in Secrets.Validate())
        {
            yield return $"Secrets.{error}";
        }
    }
}
