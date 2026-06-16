using DryIoc;
using Moongate.Core.Ids;
using Moongate.Persistence.Extensions.DryIoc;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Services.Items;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Extensions.Items;

/// <summary>
/// DryIoc-native registration helpers for UO item services.
/// </summary>
public static class ItemContainerExtensions
{
    private const ushort ItemEntityTypeId = 3;
    private const int ItemEntitySchemaVersion = 1;

    /// <summary>
    /// Registers the UO item entity persistence and the item service.
    /// </summary>
    public static IContainer AddMoongateItems(this IContainer container)
    {
        container.RegisterPersistenceEntity<ItemEntity, Serial>(ItemEntityTypeId, ItemEntitySchemaVersion, item => item.Id);
        container.Register<IItemService, ItemService>(Reuse.Singleton);
        container.Register<IItemFactoryService, ItemFactoryService>(Reuse.Singleton);
        container.Register<IContainerContentService, ContainerContentService>(Reuse.Singleton);
        container.Register<IWorldEntryService, WorldEntryService>(Reuse.Singleton);

        return container;
    }
}
