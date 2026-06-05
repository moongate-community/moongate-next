using Moongate.UO.Data.Data.Localization;
using Moongate.UO.Data.Types.Localization;

namespace Moongate.Tests.UO.Data.Localization;

public class StringEntryTests
{
    [Fact]
    public void Format_ReplacesPlaceholderWithArgument()
    {
        var entry = new StringEntry(1234, "You see ~1_val~ here.", CliLocFlagType.Original);

        Assert.Equal("You see apple here.", entry.Format("apple"));
    }

    [Fact]
    public void Format_IsThreadSafe_AcrossConcurrentCalls()
    {
        var entry = new StringEntry(1, "[~1_a~|~2_b~]", CliLocFlagType.Original);

        Parallel.For(0, 2000, i =>
        {
            var expected = $"[x{i % 2}|y{i % 2}]";
            Assert.Equal(expected, entry.Format($"x{i % 2}", $"y{i % 2}"));
        });
    }

    [Fact]
    public void Constructor_FromByteFlag_MapsToEnum()
    {
        var entry = new StringEntry(7, "text", (byte)2);

        Assert.Equal(CliLocFlagType.Modified, entry.Flag);
    }
}
