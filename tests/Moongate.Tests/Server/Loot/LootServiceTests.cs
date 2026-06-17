using Moongate.Core.Ids;
using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;
using ShaiRandom.Generators;

namespace Moongate.Tests.Server.Loot;

public sealed class LootServiceTests
{
    [Fact]
    public async Task GenerateAsync_Category_DrawsFromTag()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            9UL,
            Table("t", new LootNode { Category = "food" })
        );

        var item = Assert.Single(await service.GenerateAsync("t"));
        Assert.Contains(item.ItemId, new[] { 2512, 4155 });
    }

    [Fact]
    public async Task GenerateAsync_ChanceOne_AlwaysIncludes()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            5UL,
            Table("t", new LootNode { Item = "apple", Chance = 1.0 })
        );

        Assert.Single(await service.GenerateAsync("t"));
    }

    [Fact]
    public async Task GenerateAsync_ChanceZero_ExcludesNode()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            5UL,
            Table("t", new LootNode { Item = "apple", Chance = 0.0 })
        );

        Assert.Empty(await service.GenerateAsync("t"));
    }

    [Fact]
    public async Task GenerateAsync_Group_RollsAllChildren()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            1UL,
            Table("t", new LootNode { Item = "apple" }, new LootNode { Item = "dagger" })
        );

        var items = await service.GenerateAsync("t");

        Assert.Equal(2, items.Count);
        Assert.Contains(items, i => i.ItemId == 2512);
        Assert.Contains(items, i => i.ItemId == 3922);
    }

    [Fact]
    public async Task GenerateAsync_NonStackableWithCount_ProducesSeparateEntities()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            3UL,
            Table("t", new LootNode { Item = "apple", Amount = new LootAmount(3, 3) })
        );

        var items = await service.GenerateAsync("t");

        Assert.Equal(3, items.Count);
        Assert.All(items, i => Assert.Equal(1, i.Amount));
    }

    [Fact]
    public async Task GenerateAsync_PickOneOf_HonoursWeightsAcrossManyRolls()
    {
        var templates = Templates();
        var heavy = 0;

        for (ulong seed = 0; seed < 200; seed++)
        {
            var pick = new LootNode
            {
                PickOneOf =
                [
                    new LootNode { Item = "apple", Weight = 1 },
                    new LootNode { Item = "dagger", Weight = 9 }
                ]
            };
            var service = NewService(templates, seed, Table("t", pick));

            var item = Assert.Single(await service.GenerateAsync("t"));

            if (item.ItemId == 3922)
            {
                heavy++;
            }
        }

        // dagger weight 9 of 10 -> expect a strong majority across 200 seeded rolls.
        Assert.True(heavy > 140, $"expected dagger-heavy distribution, got {heavy}/200");
    }

    [Fact]
    public async Task GenerateAsync_PickOneOf_SelectsExactlyOne()
    {
        var templates = Templates();
        var pick = new LootNode
        {
            PickOneOf = [new LootNode { Item = "apple" }, new LootNode { Item = "dagger" }]
        };
        var service = NewService(templates, 11UL, Table("t", pick));

        Assert.Single(await service.GenerateAsync("t"));
    }

    [Fact]
    public async Task GenerateAsync_StackableWithRange_OneEntityWithinBounds()
    {
        var templates = Templates();
        var service = NewService(
            templates,
            7UL,
            Table("t", new LootNode { Item = "gold_coin", Amount = new LootAmount(1, 100) })
        );

        var items = await service.GenerateAsync("t");

        var gold = Assert.Single(items);
        Assert.Equal(3821, gold.ItemId);
        Assert.InRange(gold.Amount, 1, 100);
    }

    [Fact]
    public async Task GenerateAsync_UnknownTable_Throws()
    {
        var service = NewService(Templates(), 1UL);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateAsync("missing").AsTask());
    }

    private static LootService NewService(ItemTemplateService templates, ulong seed, params LootTableDefinition[] tables)
    {
        var factory = new FakeItemFactory(templates);
        var service = new LootService(
            templates,
            new Lazy<IItemFactoryService>(() => factory),
            new MizuchiRandom(seed, 1UL)
        );
        service.SetRegistry(new LootTableRegistry(tables, templates.GetAll()));

        return service;
    }

    private static LootTableDefinition Table(string id, params LootNode[] content)
    {
        return new LootTableDefinition { Id = id, Content = [.. content] };
    }

    private static ItemTemplateService Templates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "gold_coin", ItemId = 3821, IsStackable = true, Tags = ["currency"] },
                new ItemTemplateDefinition { Id = "apple", ItemId = 2512, Tags = ["food"] },
                new ItemTemplateDefinition { Id = "bread_loaf", ItemId = 4155, Tags = ["food"] },
                new ItemTemplateDefinition { Id = "leather_cap", ItemId = 7609, Tags = ["armor"] },
                new ItemTemplateDefinition { Id = "dagger", ItemId = 3922, Tags = ["weapon"] }
            ]
        );

        return registry;
    }

    private sealed class FakeItemFactory : IItemFactoryService
    {
        private readonly IItemTemplateService _templates;
        private uint _next = Serial.ItemOffset + 1;

        public FakeItemFactory(IItemTemplateService templates)
        {
            _templates = templates;
        }

        public List<ItemEntity> Created { get; } = [];

        public ValueTask<ItemEntity> CreateFromTemplateAsync(
            string templateId,
            CancellationToken cancellationToken = default
        )
        {
            return CreateFromTemplateAsync(templateId, 1, cancellationToken);
        }

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
                ItemId = template.ItemId,
                Amount = amount,
                IsStackable = template.IsStackable
            };

            Created.Add(item);

            return ValueTask.FromResult(item);
        }
    }
}
