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
    private const int DefaultSkillCap = 1000;

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

    public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        => _mobiles.RemoveAsync(id, cancellationToken);

    public async ValueTask<bool> EquipAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(item);

        if (mobile.EquippedItemIds.TryGetValue(layer, out var existing))
        {
            // Idempotent when the same item already sits on this layer; otherwise the layer is busy.
            return existing == item.Id;
        }

        // Detach the item from any previous owner so no dangling reference is left behind.
        await DetachAsync(mobile, item, cancellationToken);

        mobile.EquippedItemIds[layer] = item.Id;

        item.EquippedMobileId = mobile.Id;
        item.EquippedLayer = layer;
        item.ParentContainerId = default;
        item.ContainerPosition = default;

        await _mobiles.UpsertAsync(mobile, cancellationToken);
        await _items.UpsertAsync(item, cancellationToken);

        return true;
    }

    public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => _mobiles.GetByIdAsync(id, cancellationToken);

    public ValueTask<IReadOnlyList<MobileEntity>> GetByAccountIdAsync(
        Serial accountId,
        CancellationToken cancellationToken = default
    )
    {
        var result = _mobiles.Query()
                             .Where(m => m.AccountId == accountId)
                             .ToList();

        return ValueTask.FromResult<IReadOnlyList<MobileEntity>>(result);
    }

    public SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        return mobile.Skills.TryGetValue(skill, out var entry) ? entry : new();
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
            entry = new() { Cap = DefaultSkillCap, Lock = UOSkillLock.Up };
            mobile.Skills[skill] = entry;
        }

        // Sets the trained base; Value follows Base until effective-stat modifiers exist.
        // Cap and Lock are preserved across calls (only initialized for a new entry).
        entry.Base = value;
        entry.Value = value;

        await _mobiles.UpsertAsync(mobile, cancellationToken);

        return entry;
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

    private async ValueTask DetachAsync(MobileEntity targetMobile, ItemEntity item, CancellationToken cancellationToken)
    {
        // Remove the item from a previous equip slot (this mobile or another one).
        if (item.EquippedMobileId.IsValid && item.EquippedLayer is { } previousLayer)
        {
            if (item.EquippedMobileId == targetMobile.Id)
            {
                targetMobile.EquippedItemIds.Remove(previousLayer);
            }
            else
            {
                var previousMobile = await _mobiles.GetByIdAsync(item.EquippedMobileId, cancellationToken);

                if (previousMobile is not null && previousMobile.EquippedItemIds.Remove(previousLayer))
                {
                    await _mobiles.UpsertAsync(previousMobile, cancellationToken);
                }
            }
        }

        // Remove the item from its parent container, if any.
        if (item.ParentContainerId.IsValid)
        {
            var container = await _items.GetByIdAsync(item.ParentContainerId, cancellationToken);

            if (container is not null && container.ContainedItemIds.Remove(item.Id))
            {
                await _items.UpsertAsync(container, cancellationToken);
            }
        }
    }
}
