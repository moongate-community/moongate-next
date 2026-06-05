using System.Collections.Concurrent;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Server.Services.EventBus.Internal;

/// <summary>
/// Caches per-event-type handler arrays resolved from the DI container.
/// First lookup per type pays a DI resolution; subsequent lookups are a dictionary read.
/// </summary>
internal sealed class HandlerRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _asyncCache = new();
    private readonly ConcurrentDictionary<Type, object> _tickCache = new();

    public HandlerRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAsyncEventHandler<TEvent>[] ResolveAsync<TEvent>()
        where TEvent : IAsyncEvent
        => (IAsyncEventHandler<TEvent>[])_asyncCache.GetOrAdd(
            typeof(TEvent),
            static (_, sp) => sp.GetServices<IAsyncEventHandler<TEvent>>().ToArray(),
            _serviceProvider
        );

    public ITickEventHandler<TEvent>[] ResolveTick<TEvent>()
        where TEvent : ITickEvent
        => (ITickEventHandler<TEvent>[])_tickCache.GetOrAdd(
            typeof(TEvent),
            static (_, sp) => sp.GetServices<ITickEventHandler<TEvent>>().ToArray(),
            _serviceProvider
        );
}
