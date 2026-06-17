namespace Moongate.Network.UO.Data.Login;

public static class StartingCities
{
    private const int FeluccaMapId = 0;
    private const int TrammelMapId = 1;
    private const int TermurMapId = 5;

    public static readonly CityInfo[] NewHavenStartingCities =
    [
        new("New Haven", "The Bountiful Harvest Inn", 3503, 2574, 14, TrammelMapId, 1150168),
        new("Britain", "The Wayfarer's Inn", 1602, 1591, 20, TrammelMapId, 1075074)
    ];

    public static readonly CityInfo[] TrammelStartingCities =
    [
        new("Yew", "The Empath Abbey", 633, 858, 0, TrammelMapId, 1075072),
        new("Minoc", "The Barnacle", 2476, 413, 15, TrammelMapId, 1075073),
        new("Moonglow", "The Scholars Inn", 4408, 1168, 0, TrammelMapId, 1075075),
        new("Trinsic", "The Traveler's Inn", 1845, 2745, 0, TrammelMapId, 1075076),
        new("Jhelom", "The Mercenary Inn", 1374, 3826, 0, TrammelMapId, 1075078),
        new("Skara Brae", "The Falconer's Inn", 618, 2234, 0, TrammelMapId, 1075079),
        new("Vesper", "The Ironwood Inn", 2771, 976, 0, TrammelMapId, 1075080)
    ];

    public static CityInfo[] AvailableStartingCities => [.. NewHavenStartingCities, .. TrammelStartingCities];
}
