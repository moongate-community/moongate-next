using DryIoc;
using Moongate.Abstractions.Data.Seed;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Core.Types;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Seed;
using Moongate.Server.Services.Seed;
using Moongate.UO.Domain.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Extensions.Seed;

/// <summary>
/// DryIoc-native bootstrap helpers for boot-time seed actions.
/// </summary>
public static class SeedContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Adds the default first-user seed: admin/admin with administrator level when no users exist.
        /// </summary>
        public IContainer AddDefaultAdminUserSeed()
            => container.AddSeed(
                async (serviceProvider, cancellationToken) =>
                {
                    var log = Log.ForContext(typeof(SeedContainerExtensions));
                    var users = serviceProvider.GetRequiredService<IUserService>();

                    if (await users.CountAsync(cancellationToken) > 0)
                    {
                        return;
                    }

                    await users.CreateAsync(
                        "admin",
                        "admin@moongate.local",
                        "admin",
                        UserLevelType.Administrator,
                        true,
                        cancellationToken
                    );

                    log.Warning(
                        "Added default admin user with username 'admin' and password 'admin' - please change this password immediately!"
                    );
                }
            );

        /// <summary>
        /// Registers the seed service and its <see cref="ServerStartedEvent" /> handler.
        /// </summary>
        public IContainer AddMoongateSeeds()
        {
            if (!container.IsRegistered<List<SeedAction>>())
            {
                container.RegisterInstance(new List<SeedAction>());
            }

            container.RegisterDelegate<ISeedService>(
                resolver => new SeedService(
                    resolver.Resolve<IServiceProvider>(),
                    resolver.Resolve<List<SeedAction>>()
                ),
                Reuse.Singleton
            );
            container.AddTickEventHandler<SeedServerStartedHandler, ServerStartedEvent>();

            return container;
        }
    }
}
