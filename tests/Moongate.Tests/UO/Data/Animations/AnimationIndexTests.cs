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

    [Theory]
    // fileType 1 (anim.mul) unchanged by the new overload:
    [InlineData(10, 0, 0, 1, 1100)]
    [InlineData(400, 0, 0, 1, 35000)]
    // fileType 2 (anim2): body<200 -> body*110; else 22000+(body-200)*65
    [InlineData(100, 0, 0, 2, 11000)]
    [InlineData(250, 0, 0, 2, 25250)]
    // fileType 3 (anim3): body<300 -> body*110; body<400 -> 33000+(body-300)*65; else 35000+(body-400)*175
    [InlineData(100, 0, 0, 3, 11000)]
    [InlineData(350, 0, 0, 3, 36250)]
    [InlineData(400, 0, 0, 3, 35000)]
    // fileType 4 (anim4): body<200 -> body*110; body<400 -> 22000+(body-200)*65; else 35000+(body-400)*175
    [InlineData(250, 0, 0, 4, 25250)]
    [InlineData(400, 0, 0, 4, 35000)]
    // fileType 5 (anim5): body<200 && body!=34 -> body*110; else 22000+(body-200)*65
    [InlineData(100, 0, 0, 5, 11000)]
    [InlineData(250, 0, 0, 5, 25250)]
    public void GetIndex_PerFileType(int body, int action, int direction, int fileType, int expected)
    {
        Assert.Equal(expected, AnimationIndex.GetIndex(body, action, direction, fileType));
    }

    [Fact]
    public void GetIndex_DefaultFileType_MatchesFileType1()
    {
        Assert.Equal(
            AnimationIndex.GetIndex(123, 2, 3, 1),
            AnimationIndex.GetIndex(123, 2, 3)
        );
    }
}
