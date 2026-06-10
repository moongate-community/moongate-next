using Moongate.UO.Data.Data.Loadouts;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Templates.Loadouts;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// Composes and applies starter loadouts for newly created characters.
/// </summary>
public interface IStarterLoadoutService
{
    /// <summary>
    /// Creates and attaches all loadout items to the mobile: equips the backpack
    /// and equip items (applying 0xF8 shirt/pants hues where declared) and fills
    /// the backpack.
    /// </summary>
    ValueTask ApplyAsync(
        MobileEntity mobile,
        StarterLoadout loadout,
        short shirtHue,
        short pantsHue,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Composes the additive loadout for the given race index (0 human, 1 elf,
    /// 2 gargoyle) and optional profession name. Unknown race indices or
    /// profession names simply skip that overlay.
    /// </summary>
    StarterLoadout Compose(int raceIndex, string? professionName);

    /// <summary>Replaces the active loadout definition (set at boot).</summary>
    void SetDefinition(StarterLoadoutDefinition? definition);
}
