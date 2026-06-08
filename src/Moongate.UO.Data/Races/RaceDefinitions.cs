using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Races;

/// <summary>Registers <see cref="Race" /> instances into the global race registry.</summary>
public static class RaceDefinitions
{
    /// <summary>
    /// Adds (or replaces) a race in the registry by its index, keeping the indexed
    /// <see cref="Race.Races" /> slot and the <see cref="Race.AllRaces" /> list in sync so
    /// repeated registration of the same index stays idempotent.
    /// </summary>
    public static void RegisterRace(Race race)
    {
        ArgumentNullException.ThrowIfNull(race);

        var existing = Race.Races[race.RaceIndex];

        if (existing is not null)
        {
            Race.AllRaces.Remove(existing);
        }

        Race.Races[race.RaceIndex] = race;
        Race.AllRaces.Add(race);
    }
}
