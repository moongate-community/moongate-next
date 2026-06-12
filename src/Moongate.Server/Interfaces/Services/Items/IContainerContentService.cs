using Moongate.UO.Data.Entities.Items;

namespace Moongate.Server.Interfaces.Services.Items;

public interface IContainerContentService
{
    Task EnsureContentsAsync(ItemEntity container, CancellationToken cancellationToken = default);
}
