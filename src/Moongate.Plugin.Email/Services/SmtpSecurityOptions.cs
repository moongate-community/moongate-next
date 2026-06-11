using MailKit.Security;
using Moongate.Plugin.Email.Data;

namespace Moongate.Plugin.Email.Services;

/// <summary>Maps email plugin SMTP config to MailKit security options.</summary>
internal static class SmtpSecurityOptions
{
    public static SecureSocketOptions Resolve(SmtpConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.UseSsl)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        return config.StartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
    }
}
