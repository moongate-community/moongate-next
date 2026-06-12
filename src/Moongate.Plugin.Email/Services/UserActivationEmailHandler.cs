using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;
using Moongate.UO.Domain.Events;
using Serilog;

namespace Moongate.Plugin.Email.Services;

/// <summary>Sends activation emails for newly registered pending users.</summary>
public sealed class UserActivationEmailHandler : IAsyncEventHandler<UserCreatedEvent>
{
    private readonly ILogger _logger = Log.ForContext<UserActivationEmailHandler>();
    private readonly EmailPluginConfig _config;
    private readonly IEmailTemplateManager _templates;
    private readonly IEmailSender _sender;

    public UserActivationEmailHandler(
        EmailPluginConfig config,
        IEmailTemplateManager templates,
        IEmailSender sender
    )
    {
        _config = config;
        _templates = templates;
        _sender = sender;
    }

    public async Task HandleAsync(UserCreatedEvent evt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (!_config.Enabled ||
            evt.IsActive ||
            string.IsNullOrWhiteSpace(evt.Email) ||
            string.IsNullOrWhiteSpace(evt.ActivationId))
        {
            return;
        }

        try
        {
            var activationUrl = BuildActivationUrl(evt.ActivationId);
            var model = new ActivationEmailModel(evt.Username, evt.Email, evt.ActivationId, activationUrl);
            var rendered = await _templates.RenderActivationAsync(
                               _config.Activation.TemplateId,
                               model,
                               cancellationToken
                           );
            var message = new EmailMessage(evt.Username, evt.Email, rendered.Subject, rendered.TextBody, rendered.HtmlBody);

            await _sender.SendAsync(message, cancellationToken);
            _logger.Information("Activation email sent for user {UserId} ({Username})", evt.UserId, evt.Username);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Activation email failed for user {UserId} ({Username})", evt.UserId, evt.Username);
        }
    }

    private string BuildActivationUrl(string activationId)
        => _config.Activation.UrlTemplate.Replace(
            "{activation_id}",
            Uri.EscapeDataString(activationId),
            StringComparison.Ordinal
        );
}
