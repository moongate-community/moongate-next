using Moongate.Server.Data.World;
using Moongate.Server.Services.Loadouts;
using Moongate.Server.Services.Templates;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loadouts;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Loadouts;

namespace Moongate.Tests.Server.Loadouts;

public sealed class StarterLoadoutValidatorTests
{
    private const string SourceFile = "starter.yaml";

    private static ItemTemplateService NewTemplates(params ItemTemplateDefinition[] templates)
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(templates);

        return registry;
    }

    private static ProfessionDataService NewProfessions(params string[] names)
    {
        var service = new ProfessionDataService();
        service.SetProfessions(
            names.Select(static name => new ProfessionEntry(name, name, 0, 0, 0, true, 0, "Profession", [], []))
                 .ToList()
        );

        return service;
    }

    private static ItemTemplateDefinition Template(string id, ItemLayerType? layer = null, bool isAbstract = false)
        => new() { Id = id, Name = id, ItemId = 100, Layer = layer, IsAbstract = isAbstract };

    private static StarterLoadoutDefinition ValidDefinition()
    {
        var definition = new StarterLoadoutDefinition { BackpackTemplate = "backpack" };
        definition.Base.BackpackItems.Add(new() { Template = "gold_coin", Amount = 1000 });
        definition.Races["human"] = new()
        {
            EquipItems = [new() { Template = "plain_shirt", PacketHue = PacketHueSource.Shirt }]
        };
        definition.Professions["warrior"] = new()
        {
            BackpackItems = [new() { Template = "broadsword" }]
        };

        return definition;
    }

    private static ItemTemplateService ValidTemplates()
        => NewTemplates(
            Template("backpack", ItemLayerType.Backpack),
            Template("gold_coin"),
            Template("plain_shirt", ItemLayerType.Shirt),
            Template("broadsword", ItemLayerType.OneHanded)
        );

    [Fact]
    public void Validate_ValidDefinition_DoesNotThrow()
        => StarterLoadoutValidator.Validate(ValidDefinition(), SourceFile, ValidTemplates(), NewProfessions("Warrior"));

    [Fact]
    public void Validate_UnknownTemplate_Throws()
    {
        var definition = ValidDefinition();
        definition.Base.BackpackItems.Add(new() { Template = "does_not_exist" });

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
        Assert.Contains("does_not_exist", exception.Message);
    }

    [Fact]
    public void Validate_AbstractTemplate_Throws()
    {
        var templates = ValidTemplates();
        templates.UpsertRange([Template("base_clothing", isAbstract: true)]);
        var definition = ValidDefinition();
        definition.Base.BackpackItems.Add(new() { Template = "base_clothing" });

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, templates, NewProfessions("Warrior"))
        );
        Assert.Contains("abstract", exception.Message);
    }

    [Fact]
    public void Validate_EquipTemplateWithoutLayer_Throws()
    {
        var templates = ValidTemplates();
        templates.UpsertRange([Template("no_layer_item")]);
        var definition = ValidDefinition();
        definition.Base.EquipItems.Add(new() { Template = "no_layer_item" });

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, templates, NewProfessions("Warrior"))
        );
        Assert.Contains("no_layer_item", exception.Message);
        Assert.Contains("layer", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_PacketHueOnBackpackItem_Throws()
    {
        var definition = ValidDefinition();
        definition.Base.BackpackItems.Add(new() { Template = "gold_coin", PacketHue = PacketHueSource.Shirt });

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
        Assert.Contains("packet_hue", exception.Message);
    }

    [Fact]
    public void Validate_AmountBelowOne_Throws()
    {
        var definition = ValidDefinition();
        definition.Base.BackpackItems.Add(new() { Template = "gold_coin", Amount = 0 });

        Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
    }

    [Fact]
    public void Validate_UnknownRaceKey_Throws()
    {
        var definition = ValidDefinition();
        definition.Races["dwarf"] = new();

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
        Assert.Contains("dwarf", exception.Message);
    }

    [Fact]
    public void Validate_UnknownProfessionKey_Throws()
    {
        var definition = ValidDefinition();
        definition.Professions["pirate"] = new();

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
        Assert.Contains("pirate", exception.Message);
    }

    [Fact]
    public void Validate_LayerConflictAcrossSections_Throws()
    {
        var templates = ValidTemplates();
        templates.UpsertRange([Template("fancy_shirt", ItemLayerType.Shirt)]);
        var definition = ValidDefinition();
        definition.Professions["warrior"].EquipItems.Add(new() { Template = "fancy_shirt" });

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, templates, NewProfessions("Warrior"))
        );
        Assert.Contains("Shirt", exception.Message);
    }

    [Fact]
    public void Validate_BackpackItemsWithoutBackpackTemplate_Throws()
    {
        var definition = ValidDefinition();
        definition.BackpackTemplate = "";

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
        Assert.Contains("backpack_template", exception.Message);
    }

    [Fact]
    public void Validate_BackpackTemplateWithoutLayer_Throws()
    {
        var templates = NewTemplates(
            Template("backpack"),
            Template("gold_coin"),
            Template("plain_shirt", ItemLayerType.Shirt),
            Template("broadsword", ItemLayerType.OneHanded)
        );

        var exception = Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(ValidDefinition(), SourceFile, templates, NewProfessions("Warrior"))
        );
        Assert.Contains("backpack", exception.Message);
    }

    [Fact]
    public void Validate_EmptyTemplateId_Throws()
    {
        var definition = ValidDefinition();
        definition.Base.BackpackItems.Add(new() { Template = "" });

        Assert.Throws<InvalidOperationException>(
            () => StarterLoadoutValidator.Validate(definition, SourceFile, ValidTemplates(), NewProfessions("Warrior"))
        );
    }
}
