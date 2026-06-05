using Moongate.Core.Ids;

namespace Moongate.Tests.Persistence.Support;

public sealed class TestItem
{
    public Serial Id { get; set; }
    public string Label { get; set; } = "";
}
