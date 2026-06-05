using DryIoc;
using Moongate.Abstractions.Data.Persistence;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Abstractions.Internal;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Server.Extensions.Hosting;

namespace Moongate.Server.Extensions.Persistence;

/// <summary>
/// DryIoc-native bootstrap helpers for the Moongate persistence engine.
/// </summary>
public static class PersistenceContainerExtensions
{
    private const int PersistencePriority = 15;

    /// <param name="container">DryIoc container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the persistence service (snapshot + journal) with the hosting orchestrator and the
        /// open-generic <see cref="IDataAccess{TEntity,TKey}" />.
        /// </summary>
        /// <param name="saveDirectory">Directory for snapshot/journal files.</param>
        public IContainer AddMoongatePersistence(string saveDirectory)
        {
            container.AddMoongateHosting();

            container.RegisterConfigSection("persistence", () => new PersistenceConfig());

            // Ensure a (possibly empty) registration list exists even when no entity was registered.
            if (!container.IsRegistered<List<PersistenceEntityRegistration>>())
            {
                container.RegisterInstance(new List<PersistenceEntityRegistration>());
            }

            // The service ctor takes the save directory + config + accumulated registrations, so build it
            // through a delegate. Register it behind its interface only: the host container (MS DI rules)
            // produces duplicate factories for RegisterDelegate, and the strict RegisterMapping rejects
            // multiple factories — resolving through the interface uses last-registered and stays safe.
            container.RegisterDelegate<IPersistenceService>(
                resolver => new PersistenceService(
                    saveDirectory,
                    resolver.Resolve<PersistenceConfig>(),
                    resolver.Resolve<List<PersistenceEntityRegistration>>(),
                    resolver.Resolve<ITimerService>(IfUnresolved.ReturnDefault),
                    resolver.Resolve<IEventBusService>(IfUnresolved.ReturnDefault)
                ),
                Reuse.Singleton
            );

            // Drive start/stop through the orchestrator at priority 15 (after TimerWheel=3, before Network=20).
            container.RegisterDelegate(
                resolver => new MoongateServiceDescriptor(resolver.Resolve<IPersistenceService>(), PersistencePriority),
                Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
                serviceKey: typeof(PersistenceService)
            );

            // Surface persistence metrics alongside the other providers (the singleton is an IMetricProvider).
            container.RegisterDelegate<IMetricProvider>(
                resolver => (IMetricProvider)resolver.Resolve<IPersistenceService>(),
                Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation
            );

            // Open-generic IDataAccess<,> resolves through the service's GetDataAccess factory method.
            container.Register(
                typeof(IDataAccess<,>),
                made: Made.Of(
                    request => typeof(IPersistenceService).GetMethod(nameof(IPersistenceService.GetDataAccess))!
                                                          .MakeGenericMethod(request.ServiceType.GetGenericArguments()),
                    ServiceInfo.Of<IPersistenceService>()
                ),
                setup: Setup.With(asResolutionCall: true)
            );

            // Open-generic IAutoDataAccess<,> resolves through GetAutoDataAccess.
            container.Register(
                typeof(IAutoDataAccess<,>),
                made: Made.Of(
                    request => typeof(IPersistenceService).GetMethod(nameof(IPersistenceService.GetAutoDataAccess))!
                                                          .MakeGenericMethod(request.ServiceType.GetGenericArguments()),
                    ServiceInfo.Of<IPersistenceService>()
                ),
                setup: Setup.With(asResolutionCall: true)
            );

            return container;
        }
    }
}
