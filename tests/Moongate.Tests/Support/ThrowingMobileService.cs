using Moongate.Core.Ids;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.Support;

/// <summary>
/// IMobileService stub for tests that must not touch mobiles.
/// </summary>
public sealed class ThrowingMobileService : IMobileService
{
    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<bool> EquipAsync(
        MobileEntity mobile,
        ItemEntity item,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();

    public ValueTask<IReadOnlyList<MobileEntity>> GetByAccountIdAsync(
        Serial accountId,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();

    public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill)
        => throw new NotSupportedException();

    public ValueTask<SkillEntry> SetSkillAsync(
        MobileEntity mobile,
        UOSkillName skill,
        double value,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();

    public ValueTask<bool> UnequipAsync(
        MobileEntity mobile,
        ItemLayerType layer,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();
}
