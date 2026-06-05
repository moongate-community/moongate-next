using DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Internal;

namespace Moongate.Server.Extensions.Hosting;

/// <summary>
/// DryIoc-native registration helpers for Moongate services and the hosting orchestrator.
/// These replace the MEDI <c>IServiceCollection</c> helpers: ASP.NET Core registers its own
/// services through <c>IServiceCollection</c> (unavoidable in a web host), while every Moongate
/// service is registered directly on the DryIoc <see cref="IContainer" />.
/// </summary>
public static class MoongateContainerExtensions
{
    /// <summary>
    /// Default service start priority. Lower values start first.
    /// </summary>
    public const int DefaultPriority = 100;

    /// <summary>
    /// Registers the orchestrator that drives start/stop of every <see cref="IMoongateService" />.
    /// Safe to call multiple times (kept idempotent).
    /// </summary>
    /// <remarks>
    /// The orchestrator is registered as a keyed <see cref="MoongateServiceOrchestrator" /> so it
    /// can be resolved and surfaced to the generic host as an <see cref="IHostedService" /> from
    /// <c>IServiceCollection</c> (hosted services are collected from MEDI, not from native DryIoc
    /// registrations). See <c>Program.cs</c> for the bridge registration.
    /// </remarks>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateHosting(this IContainer container)
    {
        container.Register<MoongateServiceOrchestrator>(
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.Keep
        );

        return container;
    }

    /// <summary>
    /// Registers an <see cref="IMoongateService" /> behind an interface alias with a start priority.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddMoongateService<TInterface, TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TInterface : class
        where TImplementation : class, TInterface, IMoongateService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterMapping<TInterface, TImplementation>();
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    /// <summary>
    /// Registers an <see cref="IMoongateService" /> with no public interface alias.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddMoongateService<TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TImplementation : class, IMoongateService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    private static void RegisterDescriptor<TImplementation>(this IContainer container, int priority)
        where TImplementation : class, IMoongateService
        => container.RegisterDelegate(
            resolver => new MoongateServiceDescriptor(resolver.Resolve<TImplementation>(), priority),
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
            serviceKey: typeof(TImplementation)
        );
}
