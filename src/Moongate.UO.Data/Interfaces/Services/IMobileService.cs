using Moongate.Core.Ids;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Items;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
/// Provides mobile persistence plus skill access and equipment behavior.
/// </summary>
public interface IMobileService
{
    /// <summary>Returns the current number of persisted mobiles.</summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a mobile, allocating a serial when one is not already set.</summary>
    ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default);

    /// <summary>Gets a mobile by serial, or null when absent.</summary>
    ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes a mobile; returns false when absent.</summary>
    ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the mobile's skill entry, or a fresh default entry when not yet trained.
    /// Read-only: when the skill is untrained the returned entry is NOT stored, so mutating
    /// it has no effect — use <see cref="SetSkillAsync" /> to change a skill.
    /// </summary>
    SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill);

    /// <summary>Sets a skill's value (creating the entry if needed) and persists the mobile.</summary>
    ValueTask<SkillEntry> SetSkillAsync(
        MobileEntity mobile,
        UOSkillName skill,
        double value,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Equips an item on the mobile at the given layer and persists both; returns false
    /// when the layer is already occupied. Precondition: the item must not already be
    /// equipped on another mobile or held in a container — the caller must unequip/remove
    /// it first, otherwise the previous owner's reference is left dangling.
    /// </summary>
    ValueTask<bool> EquipAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes the item equipped at the given layer and persists both; returns false when
    /// the layer is empty.
    /// </summary>
    ValueTask<bool> UnequipAsync(MobileEntity mobile, ItemLayerType layer, CancellationToken cancellationToken = default);
}
