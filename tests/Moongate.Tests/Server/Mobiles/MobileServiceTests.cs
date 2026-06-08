using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Services.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileServiceTests
{
    [Fact]
    public async Task CreateAsync_AllocatesSerialInMobileRange()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());

        var mobile = await service.CreateAsync(new MobileEntity { Name = "npc" });

        Assert.True(mobile.Id.IsValid);
        Assert.True(mobile.Id.Value < Serial.ItemOffset);
    }

    [Fact]
    public async Task SetSkillAsync_CreatesEntry_AndGetSkillReturnsIt()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new MobileEntity());

        await service.SetSkillAsync(mobile, UOSkillName.Magery, 75.0);

        var skill = service.GetSkill(mobile, UOSkillName.Magery);
        Assert.Equal(75.0, skill.Value);
        Assert.Equal(75.0, skill.Base);
    }

    [Fact]
    public void GetSkill_UntrainedSkill_ReturnsDefaultEntry()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());

        var skill = service.GetSkill(new MobileEntity(), UOSkillName.Alchemy);

        Assert.Equal(0.0, skill.Value);
    }

    [Fact]
    public async Task EquipAsync_LinksMobileAndItem_ClearingContainerRefs()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var mobile = await service.CreateAsync(new MobileEntity());
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E, ParentContainerId = new(5) };
        await items.UpsertAsync(sword);

        var equipped = await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded);

        Assert.True(equipped);
        Assert.Equal(sword.Id, mobile.EquippedItemIds[ItemLayerType.OneHanded]);
        Assert.Equal(mobile.Id, sword.EquippedMobileId);
        Assert.Equal(ItemLayerType.OneHanded, sword.EquippedLayer);
        Assert.Equal(default, sword.ParentContainerId);
    }

    [Fact]
    public async Task EquipAsync_OccupiedLayer_ReturnsFalse()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new MobileEntity());
        mobile.EquippedItemIds[ItemLayerType.Helm] = new(Serial.ItemOffset + 9);

        var hat = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x1718 };

        Assert.False(await service.EquipAsync(mobile, hat, ItemLayerType.Helm));
    }

    [Fact]
    public async Task UnequipAsync_RemovesLayer_AndClearsItemBackReference()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var mobile = await service.CreateAsync(new MobileEntity());
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E };
        await items.UpsertAsync(sword);
        await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded);

        var removed = await service.UnequipAsync(mobile, ItemLayerType.OneHanded);

        Assert.True(removed);
        Assert.False(mobile.EquippedItemIds.ContainsKey(ItemLayerType.OneHanded));
        Assert.Equal(default, (await items.GetByIdAsync(sword.Id))!.EquippedMobileId);
    }

    private sealed class FakeMobileAccess : IAutoDataAccess<MobileEntity, Serial>
    {
        private readonly Dictionary<Serial, MobileEntity> _store = [];
        private uint _next = 1;

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Count);

        public ValueTask<IReadOnlyCollection<MobileEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyCollection<MobileEntity>>(_store.Values.ToArray());

        public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.TryGetValue(id, out var e) ? e : null);

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new Serial(_next++));

        public IQueryable<MobileEntity> Query() => _store.Values.AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Remove(id));

        public ValueTask UpsertAsync(MobileEntity entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeItemAccess : IAutoDataAccess<ItemEntity, Serial>
    {
        private readonly Dictionary<Serial, ItemEntity> _store = [];
        private uint _next = 1;

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Count);

        public ValueTask<IReadOnlyCollection<ItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyCollection<ItemEntity>>(_store.Values.ToArray());

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.TryGetValue(id, out var e) ? e : null);

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new Serial(_next++));

        public IQueryable<ItemEntity> Query() => _store.Values.AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Remove(id));

        public ValueTask UpsertAsync(ItemEntity entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;

            return ValueTask.CompletedTask;
        }
    }
}
