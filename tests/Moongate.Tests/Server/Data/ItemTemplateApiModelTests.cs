using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Data;

public sealed class ItemTemplateApiModelTests
{
    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues = [];

        public IReadOnlyList<Hue> Hues => _hues;

        public int Count => _hues.Count;

        public void Add(Hue hue)
            => _hues.Add(hue);

        public Hue? GetHue(int index)
            => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    [Fact]
    public void HueSummary_FromUnknownHue_ReturnsUnknownDescriptor()
    {
        var summary = HueSummary.FromValue(99, new FakeHueStore());

        Assert.False(summary.IsNone);
        Assert.False(summary.IsKnown);
        Assert.Equal("0x063", summary.Hex);
        Assert.Empty(summary.Colors);
    }

    [Fact]
    public void HueSummary_FromValidHue_ReturnsThirtyTwoRgbColors()
    {
        var store = new FakeHueStore();
        var colors = Enumerable.Repeat((ushort)0x7FFF, 32).ToArray();
        store.Add(new(colors, 0, 31, "Bright White"));

        var summary = HueSummary.FromValue(1, store);

        Assert.False(summary.IsNone);
        Assert.True(summary.IsKnown);
        Assert.Equal("Bright White", summary.Name);
        Assert.Equal("0x001", summary.Hex);
        Assert.Equal(32, summary.Colors.Count);
        Assert.Equal("#FFFFFF", summary.Colors[0].Hex);
    }

    [Fact]
    public void HueSummary_FromValueZero_ReturnsNoneDescriptor()
    {
        var summary = HueSummary.FromValue(0, new FakeHueStore());

        Assert.True(summary.IsNone);
        Assert.True(summary.IsKnown);
        Assert.Equal("0x000", summary.Hex);
        Assert.Empty(summary.Colors);
    }

    [Fact]
    public void ItemTemplateDetail_MapsParamsAndComment()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "crate",
            Name = "Crate",
            Comment = "Container base",
            BaseItem = "base_container",
            ItemId = 0x0E3F,
            GraphicVariants =
            [
                new() { ItemId = 0x0E40 }
            ],
            ScriptId = "container_script",
            Visibility = UserLevelType.GameMaster,
            Amount = 2,
            Weight = 3,
            IsMovable = true,
            IsStackable = false,
            GumpId = 42,
            Params =
            {
                ["capacity"] = new() { Type = ItemTemplateParamType.Integer, Value = "125" }
            }
        };

        var detail = ItemTemplateDetail.FromDefinition(template, new FakeHueStore());

        Assert.Equal("Container base", detail.Comment);
        Assert.Equal("base_container", detail.BaseItem);
        Assert.Equal("container_script", detail.ScriptId);
        Assert.Equal("GameMaster", detail.Visibility);
        Assert.Equal(2, detail.Amount);
        Assert.Equal(3, detail.Weight);
        Assert.True(detail.IsMovable);
        Assert.False(detail.IsStackable);
        Assert.Equal(42, detail.GumpId);
        var variant = Assert.Single(detail.GraphicVariants);
        Assert.Equal(0x0E40, variant.ItemId);
        Assert.Equal("0x0E40", variant.ItemIdHex);
        Assert.Equal("/api/items/0x0E40.png", variant.ImageUrl);
        Assert.Null(detail.Value);
        var param = Assert.Single(detail.Params);
        Assert.Equal("capacity", param.Key);
        Assert.Equal("Integer", param.Type);
        Assert.Equal("125", param.Value);
    }

    [Fact]
    public void ItemTemplateSummary_AppliesRarityValueMultiplier()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "rare_katana",
            Name = "Rare Katana",
            ItemId = 0x13FF,
            Rarity = ItemRarity.Rare,
            Value = new()
            {
                Buy = 25,
                Sell = 10
            }
        };

        var summary = ItemTemplateSummary.FromDefinition(template, new FakeHueStore());

        Assert.NotNull(summary.Value);
        Assert.Equal(1.5m, summary.Value.RarityMultiplier);
        Assert.Equal(38, summary.Value.EffectiveBuy);
        Assert.Equal(15, summary.Value.EffectiveSell);
    }

    [Fact]
    public void ItemTemplateSummary_MapsDisplayFields()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "longsword",
            Name = "Longsword",
            ItemId = 0x0F61,
            GraphicVariants =
            [
                new() { ItemId = 0x0F62 },
                new() { ItemId = 0x0F63 }
            ],
            Hue = 1,
            Rarity = ItemRarity.Common,
            Layer = ItemLayerType.OneHanded,
            Value = new()
            {
                Buy = 25,
                Sell = 12
            },
            Tags = ["weapon"],
            IsAbstract = false
        };

        var summary = ItemTemplateSummary.FromDefinition(template, new FakeHueStore());

        Assert.Equal("longsword", summary.Id);
        Assert.Equal("0x0F61", summary.ItemIdHex);
        Assert.Equal("/api/items/0x0F61.png", summary.ImageUrl);
        Assert.Collection(
            summary.GraphicVariants,
            variant =>
            {
                Assert.Equal(0x0F62, variant.ItemId);
                Assert.Equal("0x0F62", variant.ItemIdHex);
                Assert.Equal("/api/items/0x0F62.png", variant.ImageUrl);
            },
            variant =>
            {
                Assert.Equal(0x0F63, variant.ItemId);
                Assert.Equal("0x0F63", variant.ItemIdHex);
                Assert.Equal("/api/items/0x0F63.png", variant.ImageUrl);
            }
        );
        Assert.Equal("Common", summary.Rarity);
        Assert.Equal("OneHanded", summary.Layer);
        Assert.NotNull(summary.Value);
        Assert.Equal(25, summary.Value.Buy);
        Assert.Equal(12, summary.Value.Sell);
        Assert.Equal(1.0m, summary.Value.RarityMultiplier);
        Assert.Equal(25, summary.Value.EffectiveBuy);
        Assert.Equal(12, summary.Value.EffectiveSell);
        Assert.Equal(["weapon"], summary.Tags);
    }
}
