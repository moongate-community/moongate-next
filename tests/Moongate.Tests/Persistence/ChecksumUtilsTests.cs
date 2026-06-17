using Moongate.Persistence.Internal;

namespace Moongate.Tests.Persistence;

public class ChecksumUtilsTests
{
    [Fact]
    public void Compute_DifferentBytes_ProducesDifferentChecksum()
    {
        Assert.NotEqual(ChecksumUtils.Compute([1, 2, 3]), ChecksumUtils.Compute([1, 2, 4]));
    }

    [Fact]
    public void Compute_EmptySpan_DoesNotThrow()
    {
        _ = ChecksumUtils.Compute(ReadOnlySpan<byte>.Empty);
    }

    [Fact]
    public void Compute_SameBytes_ProducesSameChecksum()
    {
        byte[] data = [1, 2, 3, 4, 5];

        Assert.Equal(ChecksumUtils.Compute(data), ChecksumUtils.Compute(data));
    }
}
