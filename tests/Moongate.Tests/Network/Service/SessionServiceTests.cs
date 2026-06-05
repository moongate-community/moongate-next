using System.Net.Sockets;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Network.Client;
using Moongate.Server.Data.Events;
using Moongate.Server.Services.Network;
using ZLinq;

namespace Moongate.Tests.Network.Service;

public class SessionServiceTests
{
    private sealed class CapturingEventBusService : IEventBusService
    {
        public List<IMoongateEvent> Events { get; } = [];
        public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }
        public int CurrentTickQueueDepth => 0;

        public int DrainTickEvents(int maxItems)
            => 0;

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent
            => Events.Add(evt);

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
        {
            Events.Add(evt);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void Clear_RemovesEverySession()
    {
        var service = new SessionService();
        using var a = NewClient();
        using var b = NewClient();
        service.GetOrCreate(a);
        service.GetOrCreate(b);

        service.Clear();

        Assert.Equal(0, service.Count);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void GetOrCreate_NewSession_PublishesConnectedEventOnce()
    {
        var bus = new CapturingEventBusService();
        var service = new SessionService(bus);
        using var client = NewClient();

        var session = service.GetOrCreate(client);
        var existing = service.GetOrCreate(client);

        Assert.Same(session, existing);
        var evt = Assert.Single(bus.Events.OfType<PlayerConnectedEvent>());
        Assert.Equal(session.SessionId, evt.SessionId);
        Assert.Equal(client.RemoteEndPoint?.ToString(), evt.RemoteEndPoint);
    }

    [Fact]
    public void GetOrCreate_SameClient_ReturnsSameSession()
    {
        var service = new SessionService();
        using var client = NewClient();

        var first = service.GetOrCreate(client);
        var second = service.GetOrCreate(client);

        Assert.Same(first, second);
        Assert.Equal(1, service.Count);
        Assert.Equal(client.SessionId, first.SessionId);
    }

    [Fact]
    public void QuerySessionIds_ReturnsZLinqQueryOverSessionIds()
    {
        var service = new SessionService();
        using var a = NewClient();
        using var b = NewClient();
        var first = service.GetOrCreate(a);
        var second = service.GetOrCreate(b);

        var evenIds = service.QuerySessionIds()
                             .Where(static sessionId => sessionId % 2 == 0)
                             .ToArray();

        var expected = new[] { first.SessionId, second.SessionId }
                       .AsValueEnumerable()
                       .Where(static sessionId => sessionId % 2 == 0)
                       .Order()
                       .ToArray();

        Assert.Equal(expected, evenIds.AsValueEnumerable().Order().ToArray());
    }

    [Fact]
    public void Remove_ExistingSession_PublishesDisconnectedEventOnce()
    {
        var bus = new CapturingEventBusService();
        var service = new SessionService(bus);
        using var client = NewClient();
        var session = service.GetOrCreate(client);
        bus.Events.Clear();

        Assert.True(service.Remove(client.SessionId));
        Assert.False(service.Remove(client.SessionId));

        var evt = Assert.Single(bus.Events.OfType<PlayerDisconnectedEvent>());
        Assert.Equal(session.SessionId, evt.SessionId);
        Assert.Equal(client.RemoteEndPoint?.ToString(), evt.RemoteEndPoint);
    }

    [Fact]
    public void Remove_ExistingSession_ReturnsTrueAndDecrementsCount()
    {
        var service = new SessionService();
        using var client = NewClient();
        service.GetOrCreate(client);

        Assert.True(service.Remove(client.SessionId));
        Assert.Equal(0, service.Count);
        Assert.False(service.Remove(client.SessionId));
    }

    [Fact]
    public void TryGet_AfterCreate_ReturnsSession()
    {
        var service = new SessionService();
        using var client = NewClient();
        var created = service.GetOrCreate(client);

        Assert.True(service.TryGet(client.SessionId, out var found));
        Assert.Same(created, found);
    }

    [Fact]
    public void TryGet_UnknownId_ReturnsFalse()
    {
        var service = new SessionService();

        Assert.False(service.TryGet(12345, out _));
    }

    private static MoongateTCPClient NewClient()
        => new(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
}
