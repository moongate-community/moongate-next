using Moongate.UO.Data.Entities.Items;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// Creates persisted item entities from registered item templates.
/// </summary>
public interface IItemFactoryService
{
    /// <summary>
    /// Creates and persists an <see cref="ItemEntity" /> from the template with
    /// the given id; throws when the template is unknown or abstract.
    /// </summary>
    ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and persists an <see cref="ItemEntity" /> from the template with
    /// the given id, overriding the stack amount; throws when the template is
    /// unknown or abstract.
    /// </summary>
    ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, int amount, CancellationToken cancellationToken = default);
}
