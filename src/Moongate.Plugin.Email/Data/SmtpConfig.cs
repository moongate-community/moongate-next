using Moongate.Plugins.Configuration;
using Moongate.Plugins.Types;

namespace Moongate.Plugin.Email.Data;

/// <summary>SMTP transport settings.</summary>
public sealed class SmtpConfig
{
    /// <summary>SMTP server host.</summary>
    [ConfigField("Host", Required = true)]
    public string Host { get; set; } = "";

    /// <summary>SMTP server port.</summary>
    [ConfigField("Port", Required = true)]
    public int Port { get; set; } = 587;

    /// <summary>SMTP username.</summary>
    [ConfigField("Username", Required = true)]
    public string Username { get; set; } = "";

    /// <summary>Logical secret name resolved by ISecretManagerService.</summary>
    [ConfigField("Password secret", Required = true, Secret = true, Help = "Logical secret name resolved by the configured secret manager.")]
    public string PasswordSecret { get; set; } = "smtp_password";

    /// <summary>Connect with SSL immediately.</summary>
    [ConfigField("Use SSL")]
    public bool UseSsl { get; set; }

    /// <summary>Upgrade the connection with STARTTLS.</summary>
    [ConfigField("STARTTLS")]
    public bool StartTls { get; set; } = true;

    /// <summary>SMTP operation timeout in seconds.</summary>
    [ConfigField("Timeout seconds", Required = true)]
    public int TimeoutSeconds { get; set; } = 30;
}
