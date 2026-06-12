using Moongate.UO.Data.Animations;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class EquipConvTableTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"equipconv-{Guid.NewGuid():N}.def");

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    [Fact]
    public void MissingFile_EmptyTable()
    {
        var table = new EquipConvTable(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.def"));

        Assert.False(table.TryConvert(401, 1249, out _));
        Assert.Equal(0, table.Count);
    }

    [Fact]
    public void TryConvert_ReturnsConvertedAnimAndHue()
    {
        File.WriteAllText(
            _path,
            "# comment line\n" +
            "\"# quoted\"\n" +
            "401\t1249 1250 61250\t0\t# Human M to F\n" + // (401,1249) -> (1250, 0)
            "667\t1251 1252 61252\t5\t# garg\n" +         // (667,1251) -> (1252, 5)
            "bad line no numbers\n"
        );

        var table = new EquipConvTable(_path);

        Assert.True(table.TryConvert(401, 1249, out var a));
        Assert.Equal((1250, 0), a);
        Assert.True(table.TryConvert(667, 1251, out var b));
        Assert.Equal((1252, 5), b);
        Assert.False(table.TryConvert(401, 9999, out _)); // unknown anim
        Assert.False(table.TryConvert(999, 1249, out _)); // unknown body
    }
}
