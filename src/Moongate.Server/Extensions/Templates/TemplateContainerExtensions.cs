using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;

namespace Moongate.Server.Extensions.Templates;

/// <summary>
/// DryIoc-native registration helpers for item template services.
/// </summary>
public static class TemplateContainerExtensions
{
    private const int ItemTemplatesBootPriority = 12;

    /// <summary>
    /// Registers the item template registry, the YAML loader and the
    /// fail-fast boot service (priority 12: after world data, before network).
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateItemTemplates(this IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        container.Register<IItemTemplateService, ItemTemplateService>(Reuse.Singleton);
        container.Register<IItemTemplateAuthoringService, ItemTemplateAuthoringService>(Reuse.Singleton);
        container.RegisterDelegate(
            static resolver => new ItemTemplateYamlLoader(
                resolver.Resolve<DirectoriesConfig>()[DirectoryType.Templates_Items],
                resolver.Resolve<ITileDataStore>()
            ),
            Reuse.Singleton
        );
        container.AddMoongateHosting();
        container.AddMoongateService<ItemTemplateBootService>(ItemTemplatesBootPriority);

        return container;
    }
}
