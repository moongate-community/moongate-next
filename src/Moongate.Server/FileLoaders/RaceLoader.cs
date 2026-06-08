using Moongate.UO.Data.Races;
using Moongate.UO.Data.Races.Base;

namespace Moongate.Server.FileLoaders;

/// <summary>Registers the built-in playable races into the global race registry at boot.</summary>
public static class RaceLoader
{
    /// <summary>Registers Human (0), Elf (1) and Gargoyle (2). Safe to call more than once.</summary>
    public static void RegisterDefaultRaces()
    {
        RaceDefinitions.RegisterRace(new Human(0, 0));
        RaceDefinitions.RegisterRace(new Elf(1, 1));
        RaceDefinitions.RegisterRace(new Gargoyle(2, 2));
    }
}
