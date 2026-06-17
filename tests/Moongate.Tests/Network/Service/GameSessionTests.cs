using System.Net;
using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Compression;
using Moongate.Network.Middlewares;
using Moongate.Network.Spans;
using Moongate.Network.UO.Base;
using Moongate.Server.Services.Network.Internal;

namespace Moongate.Tests.Network.Service;

public class GameSessionTests
{
    [Fact]
    public void Ctor_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameSession(null!));
    }

    [Fact]
    public async Task EnableCompression_CompressesOutgoingPackets_AndIsIdempotent()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var receiver = new TcpClient();
        var connectTask = receiver.ConnectAsync(IPAddress.Loopback, port);
        using var senderSocket = await listener.AcceptSocketAsync();
        await connectTask;

        await using var sender = new MoongateTCPClient(senderSocket);
        var session = new GameSession(sender);

        // Enabling twice must not stack a second middleware (double compression).
        session.EnableCompression();
        session.EnableCompression();

        Assert.True(sender.ContainsMiddleware<CompressionMiddleware>());

        await session.SendPacket(new TestOutgoingPacket());

        var buffer = new byte[64];
        var read = await receiver.GetStream().ReadAsync(buffer);

        Assert.True(read > 0);
        var output = new byte[NetworkCompression.BufferSize];
        var length = NetworkCompression.Decompress(buffer.AsSpan(0, read), output);
        Assert.Equal(new byte[] { 0xAA, 0x01, 0x02 }, output[..length]);
    }

    [Fact]
    public async Task Endpoints_CapturedFromConnectedClient()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var receiver = new TcpClient();
        var connectTask = receiver.ConnectAsync(IPAddress.Loopback, port);
        using var senderSocket = await listener.AcceptSocketAsync();
        await connectTask;

        await using var sender = new MoongateTCPClient(senderSocket);
        var session = new GameSession(sender);

        Assert.NotNull(session.ServerEndPoint);
        Assert.Equal(IPAddress.Loopback, session.ServerEndPoint!.Address);
        Assert.Equal(port, session.ServerEndPoint.Port);

        Assert.NotNull(session.ClientEndPoint);
        Assert.Equal(IPAddress.Loopback, session.ClientEndPoint!.Address);
    }

    [Fact]
    public void Endpoints_UnconnectedClient_AreNull()
    {
        using var client = NewClient();

        var session = new GameSession(client);

        Assert.Null(session.ServerEndPoint);
        Assert.Null(session.ClientEndPoint);
    }

    [Fact]
    public async Task SendPacket_NullPacket_Throws()
    {
        using var client = NewClient();
        var session = new GameSession(client);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await session.SendPacket<TestOutgoingPacket>(null!));
    }

    [Fact]
    public async Task SendPacket_WritesPacketToClient()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var receiver = new TcpClient();
        var connectTask = receiver.ConnectAsync(IPAddress.Loopback, port);
        using var senderSocket = await listener.AcceptSocketAsync();
        await connectTask;

        await using var sender = new MoongateTCPClient(senderSocket);
        var session = new GameSession(sender);

        await session.SendPacket(new TestOutgoingPacket());

        var buffer = new byte[3];
        var read = await receiver.GetStream().ReadAsync(buffer);

        Assert.Equal(3, read);
        Assert.Equal(new byte[] { 0xAA, 0x01, 0x02 }, buffer);
    }

    [Fact]
    public void SessionId_MatchesOwningClient()
    {
        using var client = NewClient();

        var session = new GameSession(client);

        Assert.Equal(client.SessionId, session.SessionId);
        Assert.Same(client, session.Client);
    }

    [Fact]
    public void WithPendingBytes_NullAction_Throws()
    {
        using var client = NewClient();
        var session = new GameSession(client);

        Assert.Throws<ArgumentNullException>(() => session.WithPendingBytes(null!));
    }

    [Fact]
    public void WithPendingBytes_PersistsMutationsAcrossCalls()
    {
        using var client = NewClient();
        var session = new GameSession(client);

        session.WithPendingBytes(buffer => buffer.AddRange([1, 2, 3]));

        var observed = Array.Empty<byte>();
        session.WithPendingBytes(buffer => observed = buffer.ToArray());

        Assert.Equal(new byte[] { 1, 2, 3 }, observed);
    }

    private static MoongateTCPClient NewClient()
    {
        return new MoongateTCPClient(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
    }

    private sealed class TestOutgoingPacket : BaseGameNetworkPacket
    {
        public TestOutgoingPacket()
            : base(0xAA, 3)
        {
        }

        public override void Write(ref SpanWriter writer)
        {
            writer.Write(OpCode);
            writer.Write((byte)0x01);
            writer.Write((byte)0x02);
        }

        protected override bool ParsePayload(ref SpanReader reader)
        {
            return true;
        }
    }
}
