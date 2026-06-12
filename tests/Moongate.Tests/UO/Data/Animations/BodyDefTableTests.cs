using Moongate.UO.Data.Animations;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class BodyDefTableTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"body-def-{Guid.NewGuid():N}.def");

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
        var table = new BodyDefTable(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.def"));

        Assert.Equal((400, 0), table.Resolve(400));
    }

    [Fact]
    public void Resolve_RemapsBody_AndKeepsHue()
    {
        File.WriteAllText(
            _path,
            "# comment line\n" +
            "400 {200} 5\n" +
            "garbage line without numbers\n" +
            "500 {300, 301} 0\n"
        );

        var table = new BodyDefTable(_path);

        Assert.Equal((200, 5), table.Resolve(400));
        Assert.Equal((300, 0), table.Resolve(500)); // first id of the list
        Assert.Equal((999, 0), table.Resolve(999)); // unmapped -> itself, hue 0
    }
}
