using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Assets;

public sealed class ShippedItemTemplateYamlTests
{
    [Fact]
    public void ItemTemplates_IncludeSampleLootContainer()
    {
        var loader = new ItemTemplateYamlLoader(ItemTemplatesDirectory());

        var template = loader.LoadAll().Single(template => template.Id == "sample_loot_crate");

        Assert.Equal("Sample Loot Crate", template.Name);
        Assert.Equal("Example world-owned container that lazily generates common loot when opened.", template.Comment);
        Assert.Equal(3644, template.ItemId);
        Assert.Equal(10, template.Weight);
        Assert.False(template.IsMovable);
        Assert.Equal(60, template.GumpId);
        Assert.Contains("container", template.Tags);
        Assert.NotNull(template.Contents);
        Assert.Equal("common", template.Contents.LootTemplate);
        Assert.Equal(ItemTemplateContentGenerateType.OnOpen, template.Contents.Generate);
        Assert.Equal(TimeSpan.FromHours(6), template.Contents.RefillEvery);
        Assert.Equal(ItemTemplateContentRefillPolicy.WhenEmpty, template.Contents.RefillPolicy);
        Assert.Equal(ItemTemplateContentRefillScope.WorldOnly, template.Contents.RefillScope);
    }

    private static string ItemTemplatesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Moongate.Server", "Assets", "templates", "items");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/Moongate.Server/Assets/templates/items.");
    }
}
