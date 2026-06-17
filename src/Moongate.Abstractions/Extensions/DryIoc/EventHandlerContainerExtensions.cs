using DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Abstractions.Extensions.DryIoc;

/// <summary>
///     DryIoc-native registration helpers for Moongate event handlers.
/// </summary>
public static class EventHandlerContainerExtensions
{
    /// <summary>
    ///     Registers <typeparamref name="THandler" /> as an <see cref="IAsyncEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IContainer AddAsyncEventHandler<THandler, TEvent>(this IContainer container)
        where THandler : class, IAsyncEventHandler<TEvent>
        where TEvent : IAsyncEvent
    {
        container.Register<THandler>(Reuse.Singleton);
        container.RegisterMapping<IAsyncEventHandler<TEvent>, THandler>();

        return container;
    }

    /// <summary>
    ///     Registers <typeparamref name="THandler" /> as an <see cref="ITickEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IContainer AddTickEventHandler<THandler, TEvent>(this IContainer container)
        where THandler : class, ITickEventHandler<TEvent>
        where TEvent : ITickEvent
    {
        container.Register<THandler>(Reuse.Singleton);
        container.RegisterMapping<ITickEventHandler<TEvent>, THandler>();

        return container;
    }
}
