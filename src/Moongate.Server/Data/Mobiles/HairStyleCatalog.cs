namespace Moongate.Server.Data.Mobiles;

/// <summary>The selectable human hair and facial-hair styles (graphic ids from the race validation tables).</summary>
public static class HairStyleCatalog
{
    private static readonly IReadOnlyList<HairStyleEntry> _hair = new[]
    {
        CreateHair(0x203B, "Short"),
        CreateHair(0x203C, "Long"),
        CreateHair(0x203D, "Pony Tail"),
        CreateHair(0x2044, "Mohawk"),
        CreateHair(0x2045, "Pageboy"),
        CreateHair(0x2046, "Buns"),
        CreateHair(0x2047, "Afro"),
        CreateHair(0x2048, "Receding"),
        CreateHair(0x2049, "Two Pig Tails"),
        CreateHair(0x204A, "Topknot")
    };

    private static readonly IReadOnlyList<HairStyleEntry> _facial = new[]
    {
        CreateFacial(0x203E, "Long Beard"),
        CreateFacial(0x203F, "Short Beard"),
        CreateFacial(0x2040, "Goatee"),
        CreateFacial(0x2041, "Moustache"),
        CreateFacial(0x204B, "Medium Short Beard"),
        CreateFacial(0x204C, "Medium Long Beard"),
        CreateFacial(0x204D, "Vandyke")
    };

    public static IReadOnlyList<HairStyleEntry> Hair => _hair;

    public static IReadOnlyList<HairStyleEntry> Facial => _facial;

    private static HairStyleEntry CreateHair(int style, string name)
        => new(style, $"0x{style:X4}", name, false);

    private static HairStyleEntry CreateFacial(int style, string name)
        => new(style, $"0x{style:X4}", name, true);
}
