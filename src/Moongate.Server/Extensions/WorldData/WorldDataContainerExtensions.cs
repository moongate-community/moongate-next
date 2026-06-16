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
        RegisterDataService<IDoorDataService, DoorDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<ISpawnsDataService, SpawnsDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<ITeleportersDataService, TeleportersDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IRegionDataService, RegionDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IWeatherDataService, WeatherDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IContainerDataService, ContainerDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<ILocationCatalogService, LocationCatalogService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<INameDataService, NameDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IProfessionDataService, ProfessionDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<ISignDataService, SignDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IDecorationDataService, DecorationDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );
        RegisterDataService<IMountDataService, MountDataService>(
            container,
            resolver => new(resolver.Resolve<ServerAssetDataLoader>())
        );

        container.Register<IRegionResolverService, RegionResolverService>(Reuse.Singleton);

        container.AddMoongateHosting();
        container.AddMoongateService<ServerAssetDataBootService>(WorldDataBootPriority);

        return container;
    }

    private static void RegisterDataService<TInterface, TImplementation>(
        IContainer container,
        Func<IResolverContext, TImplementation> factory
    )
        where TInterface : class, IDataService
        where TImplementation : class, TInterface, IDataService
    {
        container.RegisterDelegate(factory, Reuse.Singleton);
        container.RegisterMapping<TInterface, TImplementation>();
        container.RegisterMapping<IDataService, TImplementation>();
    }
}
