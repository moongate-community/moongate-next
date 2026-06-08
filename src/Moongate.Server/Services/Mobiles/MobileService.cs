using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
/// Default mobile service backed by auto-increment persistence, with skill access
/// and equipment behavior that keeps mobile and item references in sync.
/// </summary>
public sealed class MobileService : IMobileService
{
    private readonly IAutoDataAccess<MobileEntity, Serial> _mobiles;
    private readonly IAutoDataAccess<ItemEntity, Serial> _items;

    public MobileService(IAutoDataAccess<MobileEntity, Serial> mobiles, IAutoDataAccess<ItemEntity, Serial> items)
    {
        _mobiles = mobiles;
        _items = items;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => _mobiles.CountAsync(cancellationToken);

    public async ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        // Mobiles occupy the low serial range; the per-type counter starts at 1, which is
        // already the mobile range, so no offset is needed (unlike items).
        if (!mobile.Id.IsValid)
        {
            mobile.Id = await _mobiles.NextIdAsync(cancellationToken);
        }

        await _mobiles.UpsertAsync(mobile, cancellationToken);

        return mobile;
    }

    public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => _mobiles.GetByIdAsync(id, cancellationToken);

    public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        => _mobiles.RemoveAsync(id, cancellationToken);

    public SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        return mobile.Skills.TryGetValue(skill, out var entry) ? entry : new SkillEntry();
    }

    public async ValueTask<SkillEntry> SetSkillAsync(
        MobileEntity mobile,
        UOSkillName skill,
        double value,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(mobile);

        if (!mobile.Skills.TryGetValue(skill, out var entry))
        {
            entry = new SkillEntry();
            mobile.Skills[skill] = entry;
        }

        entry.Value = value;
        entry.Base = value;

        await _mobiles.UpsertAsync(mobile, cancellationToken);

        return entry;
    }

    public async ValueTask<bool> EquipAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(item);

        if (mobile.EquippedItemIds.ContainsKey(layer))
        {
            return false;
        }

        mobile.EquippedItemIds[layer] = item.Id;

        item.EquippedMobileId = mobile.Id;
        item.EquippedLayer = layer;
        item.ParentContainerId = default;
        item.ContainerPosition = default;

        await _mobiles.UpsertAsync(mobile, cancellationToken);
        await _items.UpsertAsync(item, cancellationToken);

        return true;
    }

    public async ValueTask<bool> UnequipAsync(
        MobileEntity mobile,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(mobile);

        if (!mobile.EquippedItemIds.Remove(layer, out var itemId))
        {
            return false;
        }

        await _mobiles.UpsertAsync(mobile, cancellationToken);

        var item = await _items.GetByIdAsync(itemId, cancellationToken);

        if (item is not null)
        {
            item.EquippedMobileId = default;
            item.EquippedLayer = null;

            await _items.UpsertAsync(item, cancellationToken);
        }

        return true;
    }
}
