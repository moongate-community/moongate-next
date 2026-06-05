using Moongate.UO.Data.Data.Races;

namespace Moongate.UO.Data.Interfaces.Races;

/// <summary>Provides access to the UO race definitions.</summary>
public interface IRaceStore
{
    /// <summary>All loaded races.</summary>
    IReadOnlyList<RaceDefinition> Races { get; }

    /// <summary>Returns the race with the given id, or <c>null</c>.</summary>
    /// <param name="raceId">Race id.</param>
    RaceDefinition? GetById(int raceId);
}
