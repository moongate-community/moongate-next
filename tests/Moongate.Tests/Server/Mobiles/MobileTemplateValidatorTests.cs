using Moongate.Server.Services.Mobiles;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateValidatorTests
{
    private sealed class FakeLoot : Moongate.UO.Data.Interfaces.Services.ILootService
    {
        private readonly HashSet<string> _ids;

        public FakeLoot(params string[] ids)
        {
            _ids = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        }

        public bool Has(string lootTableId)
            => _ids.Contains(lootTableId);

        public ValueTask<IReadOnlyList<Moongate.UO.Data.Entities.Items.ItemEntity>> GenerateAsync(
            string lootTableId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();
    }

    private static ItemTemplateService Items()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "katana", ItemId = 5119, Layer = ItemLayerType.OneHanded },
                new ItemTemplateDefinition { Id = "backpack", ItemId = 3701, Layer = ItemLayerType.Backpack },
                new ItemTemplateDefinition { Id = "no_layer", ItemId = 1 },
                new ItemTemplateDefinition { Id = "base_armor", IsAbstract = true, Layer = ItemLayerType.InnerTorso }
            ]
        );

        return registry;
    }

    private static MobileTemplateDefinition Mob(string id)
        => new() { Id = id, Name = id };

    private static List<MobileTemplateDefinition> One(MobileTemplateDefinition m)
        => [m];

    [Fact]
    public void Validate_Valid_DoesNotThrow()
    {
        var m = Mob("guard");
        m.Equipment.Add(new MobileEquipmentEntry { Item = "katana" });
        m.BackpackTemplate = "backpack";
        m.LootTables.Add("common");
        m.Skills["Swords"] = 90;

        MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot("common"));
    }

    [Fact]
    public void Validate_UnknownEquipment_Throws()
    {
        var m = Mob("g");
        m.Equipment.Add(new MobileEquipmentEntry { Item = "missing" });

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void Validate_AbstractEquipment_Throws()
    {
        var m = Mob("g");
        m.Equipment.Add(new MobileEquipmentEntry { Item = "base_armor" });

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("abstract", ex.Message);
    }

    [Fact]
    public void Validate_EquipmentWithoutLayer_Throws()
    {
        var m = Mob("g");
        m.Equipment.Add(new MobileEquipmentEntry { Item = "no_layer" });

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("layer", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_BackpackWithoutLayer_Throws()
    {
        var m = Mob("g");
        m.BackpackTemplate = "no_layer";

        Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
    }

    [Fact]
    public void Validate_UnknownBackpack_Throws()
    {
        var m = Mob("g");
        m.BackpackTemplate = "nope";

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("nope", ex.Message);
    }

    [Fact]
    public void Validate_UnknownLootTable_Throws()
    {
        var m = Mob("g");
        m.LootTables.Add("ghost");

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("ghost", ex.Message);
    }

    [Fact]
    public void Validate_InvalidSkillName_Throws()
    {
        var m = Mob("g");
        m.Skills["NotASkill"] = 50;

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("NotASkill", ex.Message);
    }

    [Fact]
    public void Validate_InvalidNotoriety_Throws()
    {
        var m = Mob("g");
        m.Notoriety = NotorietyType.Invalid;

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("notoriety", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_BackpackNotBackpackLayer_Throws()
    {
        var m = Mob("g");
        m.BackpackTemplate = "katana"; // exists, has a layer, but it's OneHanded not Backpack

        var ex = Assert.Throws<InvalidOperationException>(() => MobileTemplateValidator.Validate(One(m), Items(), new FakeLoot()));
        Assert.Contains("Backpack", ex.Message);
    }
}
