using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// Creates persisted non-player mobile entities from registered mobile templates.
/// </summary>
public interface IMobileFactoryService
{
    /// <summary>
    /// Creates and persists a non-player <see cref="MobileEntity" /> from the
    /// template id; throws when the template is unknown or abstract.
    /// </summary>
    ValueTask<MobileEntity> CreateFromTemplateAsync(string templateId, CancellationToken cancellationToken = default);
}
