using Moongate.Abstractions.Data.Persistence;
using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.UO.Data.Data.Items;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.UO.Data.Entities.Items;

public sealed class ItemEntityPersistenceTests : IDisposable
{
    private const ushort ItemEntityTypeId = 3;
    private const int ChildCount = 50;

    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"mg-items-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Snapshot_RoundTrip_RestoresContainedItemsAndCustomProperties()
    {
        var backpackId = new Serial(Serial.ItemOffset + 1);
        var pouchId = new Serial(Serial.ItemOffset + 2);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var write = first.GetDataAccess<ItemEntity, Serial>();

        var backpack = new ItemEntity { Id = backpackId, ItemId = 0x0E75, Name = "Backpack", Weight = 3 };
        var pouch = new ItemEntity { Id = pouchId, ItemId = 0x0E79, Name = "Pouch", Weight = 1 };
        backpack.ContainedItemIds.Add(pouchId);

        await write.UpsertAsync(pouch);

        // 50 child items, each tagged with typed custom properties; the pouch holds the last two.
        for (var i = 0; i < ChildCount; i++)
        {
            var childId = new Serial(Serial.ItemOffset + 100 + (uint)i);
            var child = new ItemEntity
            {
                Id = childId,
                ItemId = 0x1BFB,
                Name = $"arrow-{i}",
                Amount = i + 1,
                Weight = 1,
                Rarity = ItemRarity.Common,
                ParentContainerId = backpackId
            };

            child.CustomProperties["index"] = new ItemCustomProperty
            {
                Type = ItemCustomPropertyType.Integer,
                IntegerValue = i
            };
            child.CustomProperties["name"] = new ItemCustomProperty
            {
                Type = ItemCustomPropertyType.String,
                StringValue = $"arrow-{i}"
            };
            child.CustomProperties["blessed"] = new ItemCustomProperty
            {
                Type = ItemCustomPropertyType.Boolean,
                BooleanValue = i % 2 == 0
            };

            if (i >= ChildCount - 2)
            {
                child.ParentContainerId = pouchId;
                pouch.ContainedItemIds.Add(childId);
            }
            else
            {
                backpack.ContainedItemIds.Add(childId);
            }

            await write.UpsertAsync(child);
        }

        await write.UpsertAsync(backpack);
        await write.UpsertAsync(pouch);
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        // Reload from disk in a fresh service.
        var second = NewService();
        await second.StartAsync(CancellationToken.None);
        var read = second.GetDataAccess<ItemEntity, Serial>();

        try
        {
            Assert.Equal(ChildCount + 2, await read.CountAsync());

            var loadedBackpack = await read.GetByIdAsync(backpackId);
            Assert.NotNull(loadedBackpack);
            Assert.Equal("Backpack", loadedBackpack!.Name);
            Assert.Equal(ChildCount - 2 + 1, loadedBackpack.ContainedItemIds.Count); // 48 arrows + pouch

            var loadedPouch = await read.GetByIdAsync(pouchId);
            Assert.NotNull(loadedPouch);
            Assert.Equal(2, loadedPouch!.ContainedItemIds.Count);

            for (var i = 0; i < ChildCount; i++)
            {
                var childId = new Serial(Serial.ItemOffset + 100 + (uint)i);
                var child = await read.GetByIdAsync(childId);

                Assert.NotNull(child);
                Assert.Equal($"arrow-{i}", child!.Name);
                Assert.Equal(i + 1, child.Amount);
                Assert.Equal(3, child.CustomProperties.Count);
                Assert.Equal(i, child.CustomProperties["index"].IntegerValue);
                Assert.Equal($"arrow-{i}", child.CustomProperties["name"].StringValue);
                Assert.Equal(i % 2 == 0, child.CustomProperties["blessed"].BooleanValue);

                var expectedParent = i >= ChildCount - 2 ? pouchId : backpackId;
                Assert.Equal(expectedParent, child.ParentContainerId);
            }
        }
        finally
        {
            await second.StopAsync(CancellationToken.None);
        }
    }

    private PersistenceService NewService()
    {
        Directory.CreateDirectory(_dir);

        var config = new PersistenceConfig { EnableFileLock = false };
        var registrations = new List<PersistenceEntityRegistration>
        {
            new(new PersistenceEntityDescriptor<ItemEntity, Serial>(ItemEntityTypeId, "ItemEntity", 1, item => item.Id))
        };

        return new PersistenceService(_dir, config, registrations);
    }
}
