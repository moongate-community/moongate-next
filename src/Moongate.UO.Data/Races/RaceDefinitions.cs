using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Races;

/// <summary>Registers <see cref="Race" /> instances into the global race registry.</summary>
public static class RaceDefinitions
{
    /// <summary>Adds (or replaces) a race in the registry by its index.</summary>
    public static void RegisterRace(Race race)
    {
        ArgumentNullException.ThrowIfNull(race);

        Race.Races[race.RaceIndex] = race;

        if (!Race.AllRaces.Contains(race))
        {
            Race.AllRaces.Add(race);
        }
    }
}
