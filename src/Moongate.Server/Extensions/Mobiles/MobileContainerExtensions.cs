using DryIoc;
using Moongate.Core.Ids;
using Moongate.Persistence.Extensions.DryIoc;
using Moongate.Server.Services.Mobiles;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Extensions.Mobiles;

/// <summary>
/// DryIoc-native registration helpers for UO mobile services.
/// </summary>
public static class MobileContainerExtensions
{
    private const ushort MobileEntityTypeId = 4;
    private const int MobileEntitySchemaVersion = 1;

    /// <summary>
    /// Registers the UO mobile entity persistence and the mobile service.
    /// </summary>
    public static IContainer AddMoongateMobiles(this IContainer container)
    {
        container.RegisterPersistenceEntity<MobileEntity, Serial>(MobileEntityTypeId, MobileEntitySchemaVersion, mobile => mobile.Id);
        container.Register<IMobileService, MobileService>(Reuse.Singleton);

        return container;
    }
}
