using Moongate.Server.Data.Templates;

namespace Moongate.Server.Interfaces.Services.Templates;

public interface IMobileTemplateAuthoringService
{
    ValueTask<MobileTemplateSaveResult> CreateAsync(
        MobileTemplateEditRequest request,
        CancellationToken cancellationToken = default
    );

    ValueTask<MobileTemplateSaveResult?> UpdateAsync(
        string id,
        MobileTemplateEditRequest request,
        CancellationToken cancellationToken = default
    );
}
