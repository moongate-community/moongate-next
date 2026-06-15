using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.UO.Data.Data.Animations;

/// <summary>Source-agnostic inputs to render a paperdoll (appearance + worn item-template ids).</summary>
public sealed record PaperdollRenderRequest(
    GenderType Gender,
    int SkinHue,
    int HairStyle,
    int HairHue,
    int FacialHairStyle,
    int FacialHairHue,
    IReadOnlyList<string> Equipment,
    bool IncludeBackground
);
