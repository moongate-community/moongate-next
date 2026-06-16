using Moongate.Core.Ids;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.Support;

/// <summary>
/// IMobileService stub that resolves the supplied mobiles by their serial (others return null).
/// </summary>
public sealed class MapItemMobileService : IMobileService
{
    private readonly Dictionary<Serial, MobileEntity> _mobiles;

    public MapItemMobileService(params MobileEntity[] mobiles)
    {
        _mobiles = mobiles.ToDictionary(static mobile => mobile.Id);
    }

    public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => new(_mobiles.GetValueOrDefault(id));

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
