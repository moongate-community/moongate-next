using DryIoc;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Extensions.WorldData;

/// <summary>
/// DryIoc-native registration helpers for server asset world data.
/// </summary>
public static class WorldDataContainerExtensions
{
    private const int WorldDataBootPriority = 11;

    /// <summary>
    /// Registers server asset world data services and startup loading.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="dataDirectory">Runtime data directory containing server world data YAML files.</param>
    public static IContainer AddMoongateWorldData(this IContainer container, string dataDirectory)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        var normalizedDataDirectory = Path.GetFullPath(dataDirectory);

        container.RegisterDelegate(_ => new ServerAssetDataLoader(normalizedDataDirectory), Reuse.Singleton);
        container.Register<IDoorDataService, DoorDataService>(Reuse.Singleton);
        container.Register<ISpawnsDataService, SpawnsDataService>(Reuse.Singleton);
        container.Register<ITeleportersDataService, TeleportersDataService>(Reuse.Singleton);
        container.Register<IRegionDataService, RegionDataService>(Reuse.Singleton);
        container.Register<IWeatherDataService, WeatherDataService>(Reuse.Singleton);
        container.Register<IContainerDataService, ContainerDataService>(Reuse.Singleton);
        container.Register<ILocationCatalogService, LocationCatalogService>(Reuse.Singleton);
        container.Register<INameDataService, NameDataService>(Reuse.Singleton);
        container.Register<IProfessionDataService, ProfessionDataService>(Reuse.Singleton);
        container.Register<ISignDataService, SignDataService>(Reuse.Singleton);
        container.Register<IDecorationDataService, DecorationDataService>(Reuse.Singleton);
        container.Register<IMountDataService, MountDataService>(Reuse.Singleton);

        container.AddMoongateHosting();
        container.AddMoongateService<ServerAssetDataBootService>(WorldDataBootPriority);

        return container;
    }
}
