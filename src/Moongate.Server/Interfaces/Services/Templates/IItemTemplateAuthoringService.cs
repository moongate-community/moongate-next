using Moongate.Server.Data.Templates;

namespace Moongate.Server.Interfaces.Services.Templates;

public interface IItemTemplateAuthoringService
{
    ValueTask<ItemTemplateSaveResult> CreateAsync(
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken = default
    );

    ValueTask<ItemTemplateSaveResult?> UpdateAsync(
        string id,
        ItemTemplateEditRequest request,
        CancellationToken cancellationToken = default
    );
}
