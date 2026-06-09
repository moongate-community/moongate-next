using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Server.Data.Config;

namespace Moongate.Server.Extensions.Configuration;

/// <summary>
/// DryIoc-native registration for the core <c>server</c> config section.
/// </summary>
public static class ServerConfigContainerExtensions
{
    /// <summary>Declares the <c>server</c> config section (server identity, e.g. shard name).</summary>
    public static IContainer AddMoongateServerConfig(this IContainer container)
    {
        container.RegisterConfigSection("server", () => new ServerConfig());

        return container;
    }
}
