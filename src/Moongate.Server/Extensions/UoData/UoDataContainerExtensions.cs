using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Core.Extensions.Directories;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.UoData;
using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Art;
using Moongate.UO.Data.Gumps;
using Moongate.UO.Data.Interfaces.Gumps;
using Moongate.UO.Data.Bodies;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Hues;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Art;
using Moongate.UO.Data.Interfaces.Bodies;
using Moongate.UO.Data.Interfaces.Expansions;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Localization;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Multi;
using Moongate.UO.Data.Interfaces.Races;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Skills;
using Moongate.UO.Data.Interfaces.Textures;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Localization;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Multi;
using Moongate.UO.Data.Races;
using Moongate.UO.Data.Skills;
using Moongate.UO.Data.Textures;
using Moongate.UO.Data.Tiles;

namespace Moongate.Server.Extensions.UoData;

/// <summary>
/// DryIoc-native registration helpers for the Moongate UO static-data layer.
/// </summary>
public static class UoDataContainerExtensions
{
    private const int UoDataBootPriority = 10;

    /// <summary>
    /// Registers the <c>uo</c> config section, the client-file resolver, the verdata patch source
    /// and the tile-data store.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateUoData(this IContainer container, string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        container.RegisterConfigSection("uo", () => new UoConfig());

        container.RegisterDelegate<IUoFileResolver>(
            resolver => new UoFileResolver(resolver.Resolve<UoConfig>().ClientFilesDirectory.ResolvePathAndEnvs()),
            Reuse.Singleton
        );

        container.Register<IVerdataPatchSource, NullVerdataPatchSource>(Reuse.Singleton);

        container.RegisterDelegate<ITileDataStore>(
            resolver => new TileDataStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IMapService>(
            resolver => new MapService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IMapImageService>(
            resolver => new MapImageService(
                resolver.Resolve<IMapService>(),
                resolver.Resolve<IUoFileResolver>(),
                resolver.Resolve<IRadarColorStore>(),
                resolver.Resolve<ITileDataStore>()
            ),
            Reuse.Singleton
        );

        container.RegisterDelegate<ILocalizationService>(
            resolver => new LocalizationService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IMultiDataStore>(
            resolver => new MultiDataStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IArtService>(
            resolver => new ArtService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IAnimationService>(
            resolver =>
            {
                var fileResolver = resolver.Resolve<IUoFileResolver>();
                var bodyDefPath = fileResolver.Resolve("Body.def") ?? Path.Combine(dataDirectory, "uo_files", "Body.def");
                var bodyConvPath = fileResolver.Resolve("Bodyconv.def") ??
                                   Path.Combine(dataDirectory, "uo_files", "Bodyconv.def");

                return new AnimationService(
                    fileResolver,
                    new(bodyDefPath),
                    new(bodyConvPath),
                    resolver.Resolve<IHueStore>()
                );
            },
            Reuse.Singleton
        );

        container.RegisterDelegate<IMobileFigureRenderer>(
            resolver =>
            {
                var fileResolver = resolver.Resolve<IUoFileResolver>();
                var equipConvPath = fileResolver.Resolve("Equipconv.def") ??
                                    Path.Combine(dataDirectory, "uo_files", "Equipconv.def");

                return new MobileFigureRenderer(
                    resolver.Resolve<IAnimationService>(),
                    resolver.Resolve<ITileDataStore>(),
                    resolver.Resolve<IItemTemplateService>(),
                    new(equipConvPath)
                );
            },
            Reuse.Singleton
        );

        container.RegisterDelegate<ISkillDataStore>(_ => new SkillDataStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IRaceStore>(_ => new RaceStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IBodyDataStore>(_ => new BodyDataStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IExpansionStore>(_ => new ExpansionStore(dataDirectory), Reuse.Singleton);

        container.RegisterDelegate<IHueStore>(
            resolver => new HueStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );
        container.RegisterDelegate<IRadarColorStore>(
            resolver => new RadarColorStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );
        container.RegisterDelegate<ITextureStore>(
            resolver => new TextureStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IGumpStore>(
            resolver => new GumpStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IPaperdollRenderer>(
            resolver => new PaperdollRenderer(
                resolver.Resolve<IGumpStore>(),
                resolver.Resolve<IItemTemplateService>(),
                resolver.Resolve<ITileDataStore>(),
                resolver.Resolve<IHueStore>()
            ),
            Reuse.Singleton
        );

        // Eager-load all UO data at boot (priority 10, before the network service at 20).
        container.AddMoongateHosting();
        container.AddMoongateService<UoDataBootService>(UoDataBootPriority);

        return container;
    }
}
