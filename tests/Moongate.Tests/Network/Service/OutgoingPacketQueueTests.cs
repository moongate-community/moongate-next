using Moongate.Network.Spans;
using Moongate.Network.UO.Base;
using Moongate.Server.Services.Network;

namespace Moongate.Tests.Network.Service;

public class OutgoingPacketQueueTests
{
    private sealed class TestOutgoingPacket : BaseGameNetworkPacket
    {
        public TestOutgoingPacket(byte opCode)
            : base(opCode, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    [Fact]
    public void Drain_InvalidMaxItems_Throws()
    {
        var queue = new OutgoingPacketQueue();

        var exception = Record.Exception(() => queue.Drain(0, _ => true));

        Assert.IsType<ArgumentOutOfRangeException>(exception);
    }

    [Fact]
    public void Drain_NullHandler_Throws()
    {
        var queue = new OutgoingPacketQueue();

        var exception = Record.Exception(() => queue.Drain(1, null!));

        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Drain_PreservesFifoOrder()
    {
        var queue = new OutgoingPacketQueue();
        queue.Enqueue(10, new TestOutgoingPacket(0xA1));
        queue.Enqueue(20, new TestOutgoingPacket(0xA2));

        var drained = new List<long>();
        var count = queue.Drain(
            10,
            envelope =>
            {
                drained.Add(envelope.SessionId);

                return true;
            }
        );

        Assert.Equal(2, count);
        Assert.Equal(new long[] { 10, 20 }, drained);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void Drain_RespectsMaxItems()
    {
        var queue = new OutgoingPacketQueue();
        queue.Enqueue(10, new TestOutgoingPacket(0xA1));
        queue.Enqueue(20, new TestOutgoingPacket(0xA2));

        var count = queue.Drain(1, _ => true);

        Assert.Equal(1, count);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void Enqueue_NullPacket_Throws()
    {
        var queue = new OutgoingPacketQueue();

        var exception = Record.Exception(() => queue.Enqueue<TestOutgoingPacket>(1, null!));

        Assert.IsType<ArgumentNullException>(exception);
    }
}
