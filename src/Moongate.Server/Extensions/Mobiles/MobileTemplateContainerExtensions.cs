using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Mobiles;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Extensions.Mobiles;

/// <summary>
/// DryIoc-native registration helpers for mobile template services.
/// </summary>
public static class MobileTemplateContainerExtensions
{
    private const int MobileTemplatesBootPriority = 16;

    /// <summary>
    /// Registers the mobile template registry, the YAML loader, the mobile factory
    /// and the fail-fast boot service (priority 16: after loot and persistence).
    /// </summary>
    public static IContainer AddMoongateMobileTemplates(this IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        container.Register<IMobileTemplateService, MobileTemplateService>(Reuse.Singleton);
        container.Register<IMobileFactoryService, MobileFactoryService>(Reuse.Singleton);
        container.RegisterDelegate(
            static resolver => new MobileTemplateYamlLoader(
                resolver.Resolve<DirectoriesConfig>()[DirectoryType.Templates_Mobiles]
            ),
            Reuse.Singleton
        );
        container.AddMoongateHosting();
        container.AddMoongateService<MobileTemplateBootService>(MobileTemplatesBootPriority);

        return container;
    }
}
