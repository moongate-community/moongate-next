namespace Moongate.UO.Data.Animations;

/// <summary>
/// Computes the <c>anim.idx</c> entry index for a body-action-direction triple, using the classic
/// per-body-type layout (monster / animal / human), 5 stored directions per action.
/// </summary>
public static class AnimationIndex
{
    private const int DirectionsPerAction = 5;

    public static int GetIndex(int body, int action, int direction)
    {
        if (body < 0 || action < 0 || direction < 0 || direction >= DirectionsPerAction)
        {
            return -1;
        }

        int baseIndex;

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

        return baseIndex + action * DirectionsPerAction + direction;
    }
}
