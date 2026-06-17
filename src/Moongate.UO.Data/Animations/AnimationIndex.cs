namespace Moongate.UO.Data.Animations;

/// <summary>
///     Computes the entry index for a body-action-direction triple within an animation file. fileType 1 is
///     <c>anim.mul</c>; 2..5 are <c>anim2.mul</c>..<c>anim5.mul</c>, each with its own body-range layout
///     (ported from the classic BodyConverter). 5 stored directions per action.
/// </summary>
public static class AnimationIndex
{
    private const int DirectionsPerAction = 5;

    public static int GetIndex(int body, int action, int direction, int fileType = 1)
    {
        if (body < 0 || action < 0 || direction < 0 || direction >= DirectionsPerAction)
        {
            return -1;
        }

        int baseIndex;

        switch (fileType)
        {
            case 2:
                baseIndex = body < 200
                    ? body * 110
                    : 22000 + (body - 200) * 65;

                break;

            case 3:
                if (body < 300)
                {
                    baseIndex = body * 110;
                }
                else if (body < 400)
                {
                    baseIndex = 33000 + (body - 300) * 65;
                }
                else
                {
                    baseIndex = 35000 + (body - 400) * 175;
                }

                break;

            case 4:
                if (body < 200)
                {
                    baseIndex = body * 110;
                }
                else if (body < 400)
                {
                    baseIndex = 22000 + (body - 200) * 65;
                }
                else
                {
                    baseIndex = 35000 + (body - 400) * 175;
                }

                break;

            case 5:
                baseIndex = body < 200 && body != 34
                    ? body * 110
                    : 22000 + (body - 200) * 65;

                break;

            default: // fileType 1 = anim.mul
                if (body < 200)
                {
                    baseIndex = body * 110;
                }
                else if (body < 400)
                {
                    baseIndex = 22000 + (body - 200) * 65;
                }
                else
                {
                    baseIndex = 35000 + (body - 400) * 175;
                }

                break;
        }

        return baseIndex + action * DirectionsPerAction + direction;
    }
}
