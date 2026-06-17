using Moongate.Server.Data.World;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
///     Provides access to profession definitions loaded from server asset data.
/// </summary>
public interface IProfessionDataService : IDataService
{
    /// <summary>
    ///     Returns all loaded professions.
    /// </summary>
    /// <returns>All loaded professions.</returns>
    IReadOnlyList<ProfessionEntry> GetAllProfessions();

    /// <summary>
    ///     Replaces all currently loaded professions.
    /// </summary>
    /// <param name="professions">Profession definitions.</param>
    void SetProfessions(IReadOnlyList<ProfessionEntry> professions);
}
