namespace Moongate.Plugin.Email.Data;

/// <summary>SMTP transport settings.</summary>
public sealed class SmtpConfig
{
    /// <summary>SMTP server host.</summary>
    public string Host { get; set; } = "";

    /// <summary>SMTP server port.</summary>
    public int Port { get; set; } = 587;

    /// <summary>SMTP username.</summary>
    public string Username { get; set; } = "";

    /// <summary>Logical secret name resolved by ISecretManagerService.</summary>
    public string PasswordSecret { get; set; } = "smtp_password";

    /// <summary>Connect with SSL immediately.</summary>
    public bool UseSsl { get; set; }

    /// <summary>Upgrade the connection with STARTTLS.</summary>
    public bool StartTls { get; set; } = true;

    /// <summary>SMTP operation timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
