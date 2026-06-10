using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IItemFactoryService stub for tests that must not create items.
/// </summary>
public sealed class ThrowingItemFactory : IItemFactoryService
{
    public ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, int amount, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
