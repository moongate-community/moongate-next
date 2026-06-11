using Moongate.Plugin.Email.Data;

namespace Moongate.Plugin.Email.Interfaces;

/// <summary>Loads and renders plugin email templates.</summary>
public interface IEmailTemplateManager
{
    /// <summary>Renders a template with the activation email model.</summary>
    ValueTask<RenderedEmailTemplate> RenderActivationAsync(
        string templateId,
        ActivationEmailModel model,
        CancellationToken cancellationToken = default
    );
}
