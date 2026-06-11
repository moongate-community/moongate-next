using Moongate.Core.Ids;
using Moongate.Server.Services.Mobiles;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Properties;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileFactoryServiceTests
{
    private sealed class FakeItemFactory : IItemFactoryService
    {
        private uint _next = Serial.ItemOffset + 1;

        public List<(string Template, Serial Id)> Created { get; } = [];

        public ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, CancellationToken cancellationToken = default)
        {
            var id = new Serial(_next++);
            var itemId = templateId switch
            {
                "katana" => 5119,
                "backpack" => 3701,
                _ => 1
            };
            var item = new ItemEntity { Id = id, ItemId = itemId, IsStackable = false };
            Created.Add((templateId, id));

            return ValueTask.FromResult(item);
        }

        public ValueTask<ItemEntity> CreateFromTemplateAsync(string templateId, int amount, CancellationToken cancellationToken = default)
            => CreateFromTemplateAsync(templateId, cancellationToken);
    }

    private sealed class FakeMobileService : IMobileService
    {
        private uint _next = 1;

        public List<(Serial MobileId, ItemLayerType Layer)> Equipped { get; } = [];

        public ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
        {
            if (!mobile.Id.IsValid)
            {
                mobile.Id = new Serial(_next++);
            }

            return ValueTask.FromResult(mobile);
        }

        public ValueTask<bool> EquipAsync(MobileEntity mobile, ItemEntity item, ItemLayerType layer, CancellationToken cancellationToken = default)
        {
            mobile.EquippedItemIds[layer] = item.Id;
            Equipped.Add((mobile.Id, layer));

            return ValueTask.FromResult(true);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<MobileEntity>> GetByAccountIdAsync(Serial accountId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Moongate.UO.Data.Data.Mobiles.SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill) => throw new NotSupportedException();
        public ValueTask<Moongate.UO.Data.Data.Mobiles.SkillEntry> SetSkillAsync(MobileEntity mobile, UOSkillName skill, double value, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<bool> UnequipAsync(MobileEntity mobile, ItemLayerType layer, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static ItemTemplateService ItemTemplates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "katana", ItemId = 5119, Layer = ItemLayerType.OneHanded },
                new ItemTemplateDefinition { Id = "backpack", ItemId = 3701, Layer = ItemLayerType.Backpack }
            ]
        );

        return registry;
    }

    private static MobileTemplateService Registry(MobileTemplateDefinition definition)
    {
        var registry = new MobileTemplateService();
        registry.UpsertRange([definition]);

        return registry;
    }

    private static MobileTemplateDefinition Guard()
    {
        var m = new MobileTemplateDefinition
        {
            Id = "town_guard",
            Name = "a guard",
            Title = "the guard",
            Body = 400,
            Gender = GenderType.Male,
            SkinHue = 1002,
            Brain = "guard_brain",
            Notoriety = Notoriety.Criminal,
            Karma = -500,
            Fame = 1200,
            FactionId = "town_britannia",
            Stats = new MobileStatsTemplate { Strength = 100, Dexterity = 90, Intelligence = 50 },
            Resources = new MobileResourcesTemplate { Hits = 120, Mana = 50, Stamina = 90 },
            Resistances = new MobileResistancesTemplate { Physical = 40 },
            BackpackTemplate = "backpack"
        };
        m.Skills["Swords"] = 90;
        m.Equipment.Add(new MobileEquipmentEntry { Item = "katana" });
        m.LootTables.Add("common");
        m.Params["faction_rank"] = new ItemTemplateParamDefinition { Type = ItemTemplateParamType.Integer, Value = "3" };

        return m;
    }

    private static (MobileFactoryService Service, FakeItemFactory Items, FakeMobileService Mobiles) New(MobileTemplateDefinition def)
    {
        var items = new FakeItemFactory();
        var mobiles = new FakeMobileService();
        var service = new MobileFactoryService(
            Registry(def),
            ItemTemplates(),
            new Lazy<IMobileService>(() => mobiles),
            new Lazy<IItemFactoryService>(() => items)
        );

        return (service, items, mobiles);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_MapsCoreFields()
    {
        var (service, _, _) = New(Guard());

        var mobile = await service.CreateFromTemplateAsync("town_guard");

        Assert.True(mobile.Id.IsValid);
        Assert.False(mobile.IsPlayer);
        Assert.True(mobile.IsAlive);
        Assert.Equal("a guard", mobile.Name);
        Assert.Equal(400, mobile.BodyId);
        Assert.Equal(GenderType.Male, mobile.Gender);
        Assert.Equal((ushort)1002, mobile.SkinHue.Value);
        Assert.Equal(100, mobile.BaseStats.Strength);
        Assert.Equal(120, mobile.Resources.Hits);
        Assert.Equal(120, mobile.Resources.MaxHits);
        Assert.Equal(40, mobile.Resistances.Physical);
        Assert.Equal(90.0, mobile.Skills[UOSkillName.Swords].Value);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_SetsBrainReputationFaction()
    {
        var (service, _, _) = New(Guard());

        var mobile = await service.CreateFromTemplateAsync("town_guard");

        Assert.Equal("guard_brain", mobile.BrainId);
        Assert.Equal(Notoriety.Criminal, mobile.Notoriety);
        Assert.Equal(-500, mobile.Karma);
        Assert.Equal(1200, mobile.Fame);
        Assert.Equal("town_britannia", mobile.FactionId);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_EquipsBackpackAndEquipment()
    {
        var (service, _, mobiles) = New(Guard());

        var mobile = await service.CreateFromTemplateAsync("town_guard");

        Assert.NotEqual(default, mobile.BackpackId);
        Assert.Contains(mobiles.Equipped, e => e.Layer == ItemLayerType.Backpack);
        Assert.Contains(mobiles.Equipped, e => e.Layer == ItemLayerType.OneHanded);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_StoresLootTablesAndParamsInCustomProperties()
    {
        var (service, _, _) = New(Guard());

        var mobile = await service.CreateFromTemplateAsync("town_guard");

        Assert.True(mobile.CustomProperties.ContainsKey(MobileTemplateDefinitionKeys.LootTables));
        Assert.Equal("common", mobile.CustomProperties[MobileTemplateDefinitionKeys.LootTables].StringValue);
        Assert.Equal(CustomPropertyType.Integer, mobile.CustomProperties["faction_rank"].Type);
        Assert.Equal(3, mobile.CustomProperties["faction_rank"].IntegerValue);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_UnknownTemplate_Throws()
    {
        var (service, _, _) = New(Guard());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateFromTemplateAsync("missing").AsTask());
    }

    [Fact]
    public async Task CreateFromTemplateAsync_AbstractTemplate_Throws()
    {
        var def = Guard();
        def.IsAbstract = true;
        var (service, _, _) = New(def);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateFromTemplateAsync("town_guard").AsTask());
    }
}
