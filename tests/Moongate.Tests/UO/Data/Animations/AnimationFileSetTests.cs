using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Interfaces.Files;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class AnimationFileSetTests
{
    private sealed class EmptyResolver : IUoFileResolver
    {
        public string RootDirectory => "";

        public bool Contains(string fileName) => false;

        public string? Resolve(string fileName) => null; // nothing installed
    }

    [Fact]
    public void Seek_WithNoFiles_ReturnsNull()
    {
        var set = new AnimationFileSet(new EmptyResolver());

        Assert.Null(set.Seek(1, 100, out var l1));
        Assert.Equal(0, l1);
        Assert.Null(set.Seek(3, 100, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Seek_UnknownFileType_ReturnsNull(int fileType)
    {
        var set = new AnimationFileSet(new EmptyResolver());

        Assert.Null(set.Seek(fileType, 100, out var len));
        Assert.Equal(0, len);
    }
}
