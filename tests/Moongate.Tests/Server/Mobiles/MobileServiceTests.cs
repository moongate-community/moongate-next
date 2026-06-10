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

        public IQueryable<MobileEntity> Query()
            => _store.Values.AsQueryable();

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

        public IQueryable<ItemEntity> Query()
            => _store.Values.AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_store.Remove(id));

        public ValueTask UpsertAsync(ItemEntity entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;

            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task CreateAsync_AllocatesSerialInMobileRange()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());

        var mobile = await service.CreateAsync(new() { Name = "npc" });

        Assert.True(mobile.Id.IsValid);
        Assert.True(mobile.Id.Value < Serial.ItemOffset);
    }

    [Fact]
    public async Task EquipAsync_ItemEquippedOnAnotherMobile_DetachesFromPreviousOwner()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var first = await service.CreateAsync(new());
        var second = await service.CreateAsync(new());
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E };
        await items.UpsertAsync(sword);

        await service.EquipAsync(first, sword, ItemLayerType.OneHanded);
        var moved = await service.EquipAsync(second, sword, ItemLayerType.OneHanded);

        Assert.True(moved);
        Assert.Equal(second.Id, sword.EquippedMobileId);
        Assert.False((await mobiles.GetByIdAsync(first.Id))!.EquippedItemIds.ContainsKey(ItemLayerType.OneHanded));
        Assert.Equal(sword.Id, (await mobiles.GetByIdAsync(second.Id))!.EquippedItemIds[ItemLayerType.OneHanded]);
    }

    [Fact]
    public async Task EquipAsync_ItemInContainer_DetachesFromContainer()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var mobile = await service.CreateAsync(new());
        var containerId = new Serial(Serial.ItemOffset + 10);
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E, ParentContainerId = containerId };
        var container = new ItemEntity { Id = containerId, ItemId = 0x0E75 };
        container.ContainedItemIds.Add(sword.Id);
        await items.UpsertAsync(sword);
        await items.UpsertAsync(container);

        var equipped = await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded);

        Assert.True(equipped);
        Assert.Equal(mobile.Id, sword.EquippedMobileId);
        Assert.Equal(default, sword.ParentContainerId);
        Assert.DoesNotContain(sword.Id, (await items.GetByIdAsync(containerId))!.ContainedItemIds);
    }

    [Fact]
    public async Task EquipAsync_LinksMobileAndItem_ClearingContainerRefs()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var mobile = await service.CreateAsync(new());
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
        var mobile = await service.CreateAsync(new());
        mobile.EquippedItemIds[ItemLayerType.Helm] = new(Serial.ItemOffset + 9);

        var hat = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x1718 };

        Assert.False(await service.EquipAsync(mobile, hat, ItemLayerType.Helm));
    }

    [Fact]
    public async Task EquipAsync_SameItemSameLayer_IsIdempotent()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new());
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E };

        Assert.True(await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded));
        Assert.True(await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded));
        Assert.Single(mobile.EquippedItemIds);
    }

    [Fact]
    public void GetSkill_UntrainedSkill_ReturnsDefaultEntry()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());

        var skill = service.GetSkill(new(), UOSkillName.Alchemy);

        Assert.Equal(0.0, skill.Value);
    }

    [Fact]
    public async Task SetSkillAsync_CreatesEntry_AndGetSkillReturnsIt()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new());

        await service.SetSkillAsync(mobile, UOSkillName.Magery, 75.0);

        var skill = service.GetSkill(mobile, UOSkillName.Magery);
        Assert.Equal(75.0, skill.Value);
        Assert.Equal(75.0, skill.Base);
    }

    [Fact]
    public async Task SetSkillAsync_ExistingSkill_PreservesCapAndLock()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new());
        mobile.Skills[UOSkillName.Swords] = new() { Value = 10, Base = 10, Cap = 1200, Lock = UOSkillLock.Locked };

        var entry = await service.SetSkillAsync(mobile, UOSkillName.Swords, 90.0);

        Assert.Equal(90.0, entry.Value);
        Assert.Equal(1200, entry.Cap);
        Assert.Equal(UOSkillLock.Locked, entry.Lock);
    }

    [Fact]
    public async Task SetSkillAsync_NewSkill_GetsDefaultCapAndLock()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());
        var mobile = await service.CreateAsync(new());

        var entry = await service.SetSkillAsync(mobile, UOSkillName.Magery, 50.0);

        Assert.Equal(1000, entry.Cap);
        Assert.Equal(UOSkillLock.Up, entry.Lock);
    }

    [Fact]
    public async Task GetByAccountIdAsync_ReturnsOnlyMobilesForThatAccount()
    {
        var mobiles = new FakeMobileAccess();
        var service = new MobileService(mobiles, new FakeItemAccess());

        var accountA = new Serial(100);
        var accountB = new Serial(200);

        var m1 = await service.CreateAsync(new() { AccountId = accountA, Name = "Alice" });
        var m2 = await service.CreateAsync(new() { AccountId = accountA, Name = "Bob" });
        await service.CreateAsync(new() { AccountId = accountB, Name = "Charlie" });

        var result = await service.GetByAccountIdAsync(accountA);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Id == m1.Id);
        Assert.Contains(result, m => m.Id == m2.Id);
    }

    [Fact]
    public async Task GetByAccountIdAsync_NoMobiles_ReturnsEmpty()
    {
        var service = new MobileService(new FakeMobileAccess(), new FakeItemAccess());

        var result = await service.GetByAccountIdAsync(new Serial(999));

        Assert.Empty(result);
    }

    [Fact]
    public async Task UnequipAsync_RemovesLayer_AndClearsItemBackReference()
    {
        var mobiles = new FakeMobileAccess();
        var items = new FakeItemAccess();
        var service = new MobileService(mobiles, items);

        var mobile = await service.CreateAsync(new());
        var sword = new ItemEntity { Id = new(Serial.ItemOffset + 1), ItemId = 0x0F5E };
        await items.UpsertAsync(sword);
        await service.EquipAsync(mobile, sword, ItemLayerType.OneHanded);

        var removed = await service.UnequipAsync(mobile, ItemLayerType.OneHanded);

        Assert.True(removed);
        Assert.False(mobile.EquippedItemIds.ContainsKey(ItemLayerType.OneHanded));
        Assert.Equal(default, (await items.GetByIdAsync(sword.Id))!.EquippedMobileId);
    }
}
