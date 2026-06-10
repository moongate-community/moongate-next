using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Server.Services.Loadouts;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loadouts;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Loadouts;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.Server.Loadouts;

public sealed class StarterLoadoutServiceTests
{
    private sealed class FakeItemFactory : IItemFactoryService
    {
        private readonly ItemTemplateService _templates;
        private uint _next = Serial.ItemOffset + 1;

        public List<ItemEntity> Created { get; } = [];

        public FakeItemFactory(ItemTemplateService templates)
        {
            _templates = templates;
        }

        public ValueTask<ItemEntity> CreateFromTemplateAsync(
            string templateId,
            CancellationToken cancellationToken = default
        )
            => CreateFromTemplateAsync(templateId, -1, cancellationToken);

        public ValueTask<ItemEntity> CreateFromTemplateAsync(
            string templateId,
            int amount,
            CancellationToken cancellationToken = default
        )
        {
            if (!_templates.TryGet(templateId, out var template))
            {
                throw new InvalidOperationException($"Item template '{templateId}' not found.");
            }

            var item = new ItemEntity
            {
                Id = new Serial(_next++),
                Name = template.Name,
                ItemId = template.ItemId,
                Amount = amount < 0 ? template.Amount : amount
            };

            Created.Add(item);

            return ValueTask.FromResult(item);
        }
    }

    private sealed class FakeMobileService : IMobileService
    {
        public List<(Serial ItemId, ItemLayerType Layer)> Equipped { get; } = [];

        public ValueTask<bool> EquipAsync(
            MobileEntity mobile,
            ItemEntity item,
            ItemLayerType layer,
            CancellationToken cancellationToken = default
        )
        {
            mobile.EquippedItemIds[layer] = item.Id;
            Equipped.Add((item.Id, layer));

            return ValueTask.FromResult(true);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
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

    private sealed class FakeItemService : IItemService
    {
        public List<(Serial ContainerId, Serial ChildId)> Added { get; } = [];

        public ValueTask<bool> AddItemAsync(
            ItemEntity container,
            ItemEntity child,
            Point2D position,
            CancellationToken cancellationToken = default
        )
        {
            container.ContainedItemIds.Add(child.Id);
            Added.Add((container.Id, child.Id));

            return ValueTask.FromResult(true);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public bool IsContainer(ItemEntity item)
            => throw new NotSupportedException();

        public bool IsContainer(int itemId)
            => throw new NotSupportedException();

        public bool IsDoor(ItemEntity item)
            => throw new NotSupportedException();

        public bool IsDoor(int itemId)
            => throw new NotSupportedException();

        public ValueTask<bool> RemoveItemAsync(
            ItemEntity container,
            Serial itemId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private static ItemTemplateService NewTemplates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "backpack", Name = "Backpack", ItemId = 3701, Layer = ItemLayerType.Backpack },
                new ItemTemplateDefinition { Id = "gold_coin", Name = "Gold", ItemId = 3821, IsStackable = true },
                new ItemTemplateDefinition { Id = "dagger", Name = "Dagger", ItemId = 3922, Layer = ItemLayerType.OneHanded },
                new ItemTemplateDefinition { Id = "plain_shirt", Name = "Shirt", ItemId = 5399, Layer = ItemLayerType.Shirt },
                new ItemTemplateDefinition { Id = "plain_pants", Name = "Pants", ItemId = 5433, Layer = ItemLayerType.Pants },
                new ItemTemplateDefinition { Id = "broadsword", Name = "Broadsword", ItemId = 3934, Layer = ItemLayerType.OneHanded }
            ]
        );

        return registry;
    }

    private static StarterLoadoutDefinition NewDefinition()
    {
        var definition = new StarterLoadoutDefinition { BackpackTemplate = "backpack" };
        definition.Base.BackpackItems.Add(new LoadoutItemEntry { Template = "gold_coin", Amount = 1000 });
        definition.Base.BackpackItems.Add(new LoadoutItemEntry { Template = "dagger" });
        definition.Races["human"] = new LoadoutSection
        {
            EquipItems =
            [
                new LoadoutItemEntry { Template = "plain_shirt", PacketHue = PacketHueSource.Shirt },
                new LoadoutItemEntry { Template = "plain_pants", PacketHue = PacketHueSource.Pants }
            ]
        };
        definition.Professions["warrior"] = new LoadoutSection
        {
            BackpackItems = [new LoadoutItemEntry { Template = "broadsword" }]
        };

        return definition;
    }

    private static (StarterLoadoutService Service, FakeItemFactory Factory, FakeMobileService Mobiles, FakeItemService Items)
        NewService(StarterLoadoutDefinition? definition)
    {
        var templates = NewTemplates();
        var factory = new FakeItemFactory(templates);
        var mobiles = new FakeMobileService();
        var items = new FakeItemService();
        var service = new StarterLoadoutService(
            templates,
            new Lazy<IItemFactoryService>(() => factory),
            new Lazy<IMobileService>(() => mobiles),
            new Lazy<IItemService>(() => items)
        );
        service.SetDefinition(definition);

        return (service, factory, mobiles, items);
    }

    [Fact]
    public void Compose_NoDefinition_ReturnsEmptyLoadout()
    {
        var (service, _, _, _) = NewService(null);

        var loadout = service.Compose(0, "warrior");

        Assert.True(loadout.IsEmpty);
    }

    [Fact]
    public void Compose_BaseOnly_ResolvesBackpackAndBaseItems()
    {
        var (service, _, _, _) = NewService(NewDefinition());

        var loadout = service.Compose(raceIndex: 5, professionName: null);

        Assert.NotNull(loadout.Backpack);
        Assert.Equal("backpack", loadout.Backpack.Template.Id);
        Assert.Equal(ItemLayerType.Backpack, loadout.Backpack.Layer);
        Assert.Equal(2, loadout.BackpackItems.Count);
        Assert.Empty(loadout.Equip);
    }

    [Fact]
    public void Compose_WithRace_AddsRaceEquip()
    {
        var (service, _, _, _) = NewService(NewDefinition());

        var loadout = service.Compose(raceIndex: 0, professionName: null);

        Assert.Equal(2, loadout.Equip.Count);
        Assert.Equal(ItemLayerType.Shirt, loadout.Equip[0].Layer);
        Assert.Equal(PacketHueSource.Shirt, loadout.Equip[0].PacketHue);
    }

    [Fact]
    public void Compose_WithRaceAndProfession_AddsAllOverlays()
    {
        var (service, _, _, _) = NewService(NewDefinition());

        var loadout = service.Compose(raceIndex: 0, professionName: "warrior");

        Assert.Equal(2, loadout.Equip.Count);
        Assert.Equal(3, loadout.BackpackItems.Count);
        Assert.Contains(loadout.BackpackItems, item => item.Template.Id == "broadsword");
    }

    [Fact]
    public void Compose_UnknownProfession_NoProfessionOverlay()
    {
        var (service, _, _, _) = NewService(NewDefinition());

        var loadout = service.Compose(raceIndex: 0, professionName: "pirate");

        Assert.Equal(2, loadout.BackpackItems.Count);
    }

    [Fact]
    public void Compose_AmountOverride_WinsOverTemplateAmount()
    {
        var (service, _, _, _) = NewService(NewDefinition());

        var loadout = service.Compose(raceIndex: 0, professionName: null);

        var gold = loadout.BackpackItems.Single(item => item.Template.Id == "gold_coin");
        Assert.Equal(1000, gold.Amount);
        var dagger = loadout.BackpackItems.Single(item => item.Template.Id == "dagger");
        Assert.Equal(1, dagger.Amount);
    }

    [Fact]
    public async Task ApplyAsync_EquipsBackpackAndSetsBackpackId()
    {
        var (service, factory, mobiles, _) = NewService(NewDefinition());
        var mobile = new MobileEntity { Id = new Serial(1) };
        var loadout = service.Compose(0, null);

        await service.ApplyAsync(mobile, loadout, 0, 0);

        var backpack = factory.Created.Single(item => item.ItemId == 3701);
        Assert.Equal(backpack.Id, mobile.BackpackId);
        Assert.Contains(mobiles.Equipped, pair => pair.ItemId == backpack.Id && pair.Layer == ItemLayerType.Backpack);
    }

    [Fact]
    public async Task ApplyAsync_EquipsItemsOnTemplateLayers()
    {
        var (service, _, mobiles, _) = NewService(NewDefinition());
        var mobile = new MobileEntity { Id = new Serial(1) };
        var loadout = service.Compose(0, null);

        await service.ApplyAsync(mobile, loadout, 0, 0);

        Assert.Contains(mobiles.Equipped, pair => pair.Layer == ItemLayerType.Shirt);
        Assert.Contains(mobiles.Equipped, pair => pair.Layer == ItemLayerType.Pants);
    }

    [Fact]
    public async Task ApplyAsync_AppliesPacketHuesOnlyToDeclaredEntries()
    {
        var (service, factory, _, _) = NewService(NewDefinition());
        var mobile = new MobileEntity { Id = new Serial(1) };
        var loadout = service.Compose(0, null);

        await service.ApplyAsync(mobile, loadout, shirtHue: 33, pantsHue: 44);

        var shirt = factory.Created.Single(item => item.ItemId == 5399);
        var pants = factory.Created.Single(item => item.ItemId == 5433);
        var backpack = factory.Created.Single(item => item.ItemId == 3701);
        Assert.Equal((ushort)33, shirt.Hue.Value);
        Assert.Equal((ushort)44, pants.Hue.Value);
        Assert.Equal((ushort)0, backpack.Hue.Value);
    }

    [Fact]
    public async Task ApplyAsync_ZeroPacketHue_KeepsTemplateHue()
    {
        var (service, factory, _, _) = NewService(NewDefinition());
        var mobile = new MobileEntity { Id = new Serial(1) };
        var loadout = service.Compose(0, null);

        await service.ApplyAsync(mobile, loadout, shirtHue: 0, pantsHue: 0);

        var shirt = factory.Created.Single(item => item.ItemId == 5399);
        Assert.Equal((ushort)0, shirt.Hue.Value);
    }

    [Fact]
    public async Task ApplyAsync_AddsBackpackItemsToBackpackWithAmounts()
    {
        var (service, factory, _, items) = NewService(NewDefinition());
        var mobile = new MobileEntity { Id = new Serial(1) };
        var loadout = service.Compose(0, "warrior");

        await service.ApplyAsync(mobile, loadout, 0, 0);

        var backpack = factory.Created.Single(item => item.ItemId == 3701);
        Assert.Equal(3, items.Added.Count);
        Assert.All(items.Added, pair => Assert.Equal(backpack.Id, pair.ContainerId));
        var gold = factory.Created.Single(item => item.ItemId == 3821);
        Assert.Equal(1000, gold.Amount);
    }

    [Fact]
    public async Task ApplyAsync_EmptyLoadout_DoesNothing()
    {
        var (service, factory, mobiles, items) = NewService(null);
        var mobile = new MobileEntity { Id = new Serial(1) };

        await service.ApplyAsync(mobile, service.Compose(0, null), 0, 0);

        Assert.Empty(factory.Created);
        Assert.Empty(mobiles.Equipped);
        Assert.Empty(items.Added);
    }
}
