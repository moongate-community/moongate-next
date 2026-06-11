using Moongate.UO.Data.Animations;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class AnimationIndexTests
{
    [Theory]
    [InlineData(10, 0, 0, 1100)]    // monster: 10*110 + 0 + 0
    [InlineData(10, 1, 2, 1107)]    // monster: 10*110 + 1*5 + 2
    [InlineData(200, 0, 0, 22000)]  // animal base
    [InlineData(201, 0, 1, 22066)]  // 22000 + 1*65 + 0*5 + 1
    [InlineData(400, 0, 0, 35000)]  // human base
    [InlineData(401, 2, 3, 35188)]  // 35000 + 1*175 + 2*5 + 3
    public void GetIndex_KnownValues(int body, int action, int direction, int expected)
    {
        Assert.Equal(expected, AnimationIndex.GetIndex(body, action, direction));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(10, -1, 0)]
    [InlineData(10, 0, -1)]
    [InlineData(10, 0, 5)]
    public void GetIndex_OutOfRange_ReturnsNegative(int body, int action, int direction)
    {
        Assert.True(AnimationIndex.GetIndex(body, action, direction) < 0);
    }
}
