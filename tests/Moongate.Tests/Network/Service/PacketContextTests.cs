using Moongate.Abstractions.Data.Network;
using Moongate.Network.Spans;
using Moongate.Network.UO.Base;

namespace Moongate.Tests.Network.Service;

public class PacketContextTests
{
    private sealed class TestPacket : BaseGameNetworkPacket
    {
        public TestPacket(byte opCode)
            : base(opCode, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    [Fact]
    public async Task BroadcastAsync_EnqueuesToAllSessionsIncludingSelf()
    {
        var sent = new List<long>();
        var context = NewContext(10, [10, 20, 30], sent);

        await context.BroadcastAsync(new TestPacket(0xA1));

        Assert.Equal(new long[] { 10, 20, 30 }, sent);
    }

    [Fact]
    public async Task BroadcastExceptSelfAsync_EnqueuesToAllSessionsExceptSelf()
    {
        var sent = new List<long>();
        var context = NewContext(10, [10, 20, 30], sent);

        await context.BroadcastExceptSelfAsync(new TestPacket(0xA1));

        Assert.Equal(new long[] { 20, 30 }, sent);
    }

    [Fact]
    public void Ctor_NullPacket_Throws()
    {
        var exception = Record.Exception(
            () => new PacketContext<TestPacket>(
                10,
                null!,
                DateTimeOffset.UtcNow,
                (_, _, _) => Task.CompletedTask,
                () => []
            )
        );

        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public async Task SendAsync_EnqueuesToCurrentSession()
    {
        var sent = new List<long>();
        var context = NewContext(10, [10, 20], sent);

        await context.SendAsync(new TestPacket(0xA1));

        Assert.Equal(new long[] { 10 }, sent);
    }

    [Fact]
    public async Task SendAsync_NullPacket_Throws()
    {
        var context = NewContext(10, [10, 20], []);

        var exception = await Record.ExceptionAsync(() => context.SendAsync<TestPacket>(null!));

        Assert.IsType<ArgumentNullException>(exception);
    }

    private static PacketContext<TestPacket> NewContext(long sessionId, long[] sessions, List<long> sent)
        => new(
            sessionId,
            new(0x01),
            DateTimeOffset.UtcNow,
            (targetSessionId, _, _) =>
            {
                sent.Add(targetSessionId);

                return Task.CompletedTask;
            },
            () => sessions
        );
}
