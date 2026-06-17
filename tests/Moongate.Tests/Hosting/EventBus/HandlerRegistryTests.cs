using Microsoft.Extensions.DependencyInjection;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Server.Services.EventBus.Internal;
using Moongate.Tests.Hosting.EventBus.Support;

namespace Moongate.Tests.Hosting.EventBus;

public class HandlerRegistryTests
{
    [Fact]
    public void ResolveAsync_CachedAcrossCalls_ReturnsSameArrayInstance()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(new CountingAsyncHandler());
        var sp = services.BuildServiceProvider();
        var registry = new HandlerRegistry(sp);

        var first = registry.ResolveAsync<TestAsyncEvent>();
        var second = registry.ResolveAsync<TestAsyncEvent>();

        Assert.Same(first, second);
    }

    [Fact]
    public void ResolveAsync_MultipleHandlersRegistered_PreservesRegistrationOrder()
    {
        var first = new CountingAsyncHandler();
        var second = new CountingAsyncHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(first);
        services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(second);
        var sp = services.BuildServiceProvider();
        var registry = new HandlerRegistry(sp);

        var handlers = registry.ResolveAsync<TestAsyncEvent>();

        Assert.Equal(2, handlers.Length);
        Assert.Same(first, handlers[0]);
        Assert.Same(second, handlers[1]);
    }

    [Fact]
    public void ResolveAsync_NoHandlersRegistered_ReturnsEmptyArray()
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var registry = new HandlerRegistry(sp);

        var handlers = registry.ResolveAsync<TestAsyncEvent>();

        Assert.Empty(handlers);
    }

    [Fact]
    public void ResolveAsync_SingleHandlerRegistered_ReturnsIt()
    {
        var services = new ServiceCollection();
        var handler = new CountingAsyncHandler();
        services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(handler);
        var sp = services.BuildServiceProvider();
        var registry = new HandlerRegistry(sp);

        var handlers = registry.ResolveAsync<TestAsyncEvent>();

        Assert.Single(handlers);
        Assert.Same(handler, handlers[0]);
    }

    [Fact]
    public void ResolveTick_MirrorBehavior_ForTickHandlers()
    {
        var services = new ServiceCollection();
        var handler = new CountingTickHandler();
        services.AddSingleton<ITickEventHandler<TestTickEvent>>(handler);
        var sp = services.BuildServiceProvider();
        var registry = new HandlerRegistry(sp);

        var handlers = registry.ResolveTick<TestTickEvent>();

        Assert.Single(handlers);
        Assert.Same(handler, handlers[0]);
    }

    private sealed class CountingAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
    {
        public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class CountingTickHandler : ITickEventHandler<TestTickEvent>
    {
        public void Handle(TestTickEvent evt)
        {
        }
    }
}
