using Moongate.UO.Data.Animations;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class BodyConvTableTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"bodyconv-{Guid.NewGuid():N}.def");

    [Fact]
    public void TryRoute_PicksFirstNonNegativeColumn()
    {
        File.WriteAllText(
            _path,
            "# comment line\n" +
            "\"# quoted comment\"\n" +
            "157\t1\t-1\t-1\t-1\t-1\n" +   // anim2 -> fileType 2, index 1
            "11\t-1\t3\t-1\t-1\t-1\n" +    // anim3 -> fileType 3, index 3
            "20\t-1\t-1\t-1\t9\t-1\n" +    // anim5 -> fileType 5, index 9
            "50\t-1\t-1\t-1\t-1\t-1\n" +   // all -1 -> not routed
            "garbage line\n"
        );

        var table = new BodyConvTable(_path);

        Assert.True(table.TryRoute(157, out var r157));
        Assert.Equal((2, 1), r157);
        Assert.True(table.TryRoute(11, out var r11));
        Assert.Equal((3, 3), r11);
        Assert.True(table.TryRoute(20, out var r20));
        Assert.Equal((5, 9), r20);
        Assert.False(table.TryRoute(50, out _));   // all columns -1
        Assert.False(table.TryRoute(999, out _));  // absent
    }

    [Fact]
    public void MissingFile_EmptyTable()
    {
        var table = new BodyConvTable(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.def"));

        Assert.False(table.TryRoute(157, out _));
        Assert.Equal(0, table.Count);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
