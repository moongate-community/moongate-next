using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Server.World;

public sealed class WorldEntitiesBootServiceTests
{
    [Fact]
    public async Task StartAsync_AddsNpcsAndGroundItems_SkipsPlayersAndContainedItems()
    {
        var npc = new MobileEntity { Id = new Serial(1), MapId = 0, Location = new Point3D(50, 50, 0), IsPlayer = false };
        var player = new MobileEntity { Id = new Serial(2), MapId = 0, Location = new Point3D(50, 50, 0), IsPlayer = true };
        var groundItem = new ItemEntity { Id = new Serial(0x40000001), MapId = 0, Location = new Point3D(51, 50, 0) };
        var containedItem = new ItemEntity
        {
            Id = new Serial(0x40000002), MapId = 0, Location = new Point3D(51, 50, 0),
            ParentContainerId = new Serial(0x40000099)
        };

        var index = new WorldSpatialIndex();
        var service = new WorldEntitiesBootService(
            new FakeDataAccess<MobileEntity>([npc, player]),
            new FakeDataAccess<ItemEntity>([groundItem, containedItem]),
            index
        );

        await service.StartAsync(CancellationToken.None);

        Assert.True(index.TryGet(new Serial(1), out _));                                  // NPC added
        Assert.False(index.TryGet(new Serial(2), out _));                                 // player NOT added
        Assert.Single(index.All);                                                         // only the NPC is a tracked mobile
        Assert.Contains(groundItem, index.GetItemsInRange(0, new Point3D(51, 50, 0), 1)); // ground item added
        Assert.DoesNotContain(
            containedItem,
            index.GetItemsInRange(0, new Point3D(51, 50, 0), 1)
        ); // contained item NOT added
    }

    // Minimal IAutoDataAccess double: GetAllAsync returns the configured list; everything else throws (unused).
    private sealed class FakeDataAccess<TEntity> : IAutoDataAccess<TEntity, Serial>
        where TEntity : class
    {
        private readonly IReadOnlyCollection<TEntity> _all;

        public FakeDataAccess(IReadOnlyCollection<TEntity> all)
        {
            _all = all;
        }

        public ValueTask<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_all);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<TEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public IQueryable<TEntity> Query()
        {
            throw new NotSupportedException();
        }

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
