namespace Moongate.UO.Data.Animations;

/// <summary>The inputs needed to render a dressed mobile figure (body + skin/hair hues + hair styles).</summary>
public sealed record MobileRenderRequest(
    int Body,
    int SkinHue,
    int HairStyle,
    int HairHue,
    int FacialHairStyle,
    int FacialHairHue,
    IReadOnlyList<string> Equipment
);
