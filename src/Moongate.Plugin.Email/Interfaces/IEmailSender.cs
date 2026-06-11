using Moongate.Plugin.Email.Data;

namespace Moongate.Plugin.Email.Interfaces;

/// <summary>Sends rendered email messages.</summary>
public interface IEmailSender
{
    /// <summary>Sends the provided email message.</summary>
    ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
