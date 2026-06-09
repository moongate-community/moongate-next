using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Services.LiveConsole;
using Moongate.Server.Types.LiveConsole;

namespace Moongate.Tests.Server.LiveConsole;

public class LiveConsoleBroadcasterTests
{
    [Fact]
    public void GetBacklog_PreservesChronologicalOrder()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        broadcaster.Publish(Entry(0));
        broadcaster.Publish(Entry(1));
        broadcaster.Publish(Entry(2));

        Assert.Equal(new[] { "0", "1", "2" }, broadcaster.GetBacklog().Select(e => e.Message));
    }

    [Fact]
    public void Publish_OverCapacity_KeepsLast200()
    {
        var broadcaster = new LiveConsoleBroadcaster();

        for (var i = 0; i < 250; i++)
        {
            broadcaster.Publish(Entry(i));
        }

        var backlog = broadcaster.GetBacklog();

        Assert.Equal(200, backlog.Count);
        Assert.Equal("50", backlog[0].Message);
        Assert.Equal("249", backlog[199].Message);
    }

    [Fact]
    public void Publish_RaisesEntryPublished()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        LiveConsoleEntry? received = null;
        broadcaster.EntryPublished += e => received = e;

        var entry = Entry(7);
        broadcaster.Publish(entry);

        Assert.Same(entry, received);
    }

    [Fact]
    public void Publish_ThrowingSubscriber_DoesNotPropagate()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        broadcaster.EntryPublished += _ => throw new InvalidOperationException("bad subscriber");

        var ex = Record.Exception(() => broadcaster.Publish(Entry(0)));

        Assert.Null(ex);
    }

    private static LiveConsoleEntry Entry(int i)
        => new() { Kind = LiveConsoleEntryKind.Log, Timestamp = i, Message = i.ToString() };
}
