using Moongate.Abstractions.Data.Persistence;
using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Properties;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.UO.Data.Entities.Mobiles;

public sealed class MobileEntityPersistenceTests : IDisposable
{
    private const ushort MobileEntityTypeId = 4;

    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"mg-mobiles-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Snapshot_RoundTrip_RestoresBrainReputationAndFaction()
    {
        var mobileId = new Serial(3);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<MobileEntity, Serial>().UpsertAsync(
            new MobileEntity
            {
                Id = mobileId,
                Name = "a guard",
                BrainId = "guard_brain",
                Notoriety = Notoriety.Criminal,
                Karma = -500,
                Fame = 1200,
                FactionId = "town_britannia"
            }
        );
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        try
        {
            var loaded = await second.GetDataAccess<MobileEntity, Serial>().GetByIdAsync(mobileId);

            Assert.NotNull(loaded);
            Assert.Equal("guard_brain", loaded!.BrainId);
            Assert.Equal(Notoriety.Criminal, loaded.Notoriety);
            Assert.Equal(-500, loaded.Karma);
            Assert.Equal(1200, loaded.Fame);
            Assert.Equal("town_britannia", loaded.FactionId);
        }
        finally
        {
            await second.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Snapshot_RoundTrip_CustomPropertyKeysStayCaseInsensitive()
    {
        var mobileId = new Serial(2);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var mobile = new MobileEntity { Id = mobileId, Name = "Caseless" };
        mobile.CustomProperties["Origin"] = new() { Type = CustomPropertyType.String, StringValue = "seeded" };
        await first.GetDataAccess<MobileEntity, Serial>().UpsertAsync(mobile);
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        try
        {
            var loaded = await second.GetDataAccess<MobileEntity, Serial>().GetByIdAsync(mobileId);

            Assert.NotNull(loaded);

            // Lookup with a different case must still resolve after the round-trip.
            Assert.True(loaded!.CustomProperties.ContainsKey("ORIGIN"));
            Assert.Equal("seeded", loaded.CustomProperties["origin"].StringValue);
        }
        finally
        {
            await second.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Snapshot_RoundTrip_RestoresStatsSkillsEquipmentAndCustomProperties()
    {
        var mobileId = new Serial(1);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var write = first.GetDataAccess<MobileEntity, Serial>();

        var mobile = new MobileEntity
        {
            Id = mobileId,
            Name = "Arthorius",
            Title = "the Brave",
            AccountId = new(7),
            BodyId = 0x190,
            Gender = GenderType.Male,
            RaceIndex = 0,
            SkinHue = (Hue)0x83EA,
            HairStyle = 0x203B,
            HairHue = (Hue)0x47E,
            IsPlayer = true,
            StatCap = 225,
            BaseStats = { Strength = 100, Dexterity = 90, Intelligence = 35 },
            Resistances = { Physical = 70, Fire = 40, Cold = 35, Poison = 30, Energy = 25 },
            Resources = { Hits = 95, MaxHits = 100, Mana = 10, MaxMana = 35, Stamina = 80, MaxStamina = 90 },
            BackpackId = new(Serial.ItemOffset + 1)
        };

        mobile.Skills[UOSkillName.Swords] = new() { Value = 99.5, Base = 99.5, Cap = 1000, Lock = UOSkillLock.Up };
        mobile.Skills[UOSkillName.Tactics] = new() { Value = 80.0, Base = 80.0, Cap = 1000, Lock = UOSkillLock.Locked };
        mobile.EquippedItemIds[ItemLayerType.OneHanded] = new(Serial.ItemOffset + 2);
        mobile.EquippedItemIds[ItemLayerType.Helm] = new(Serial.ItemOffset + 3);
        mobile.CustomProperties["origin"] = new() { Type = CustomPropertyType.String, StringValue = "seeded" };

        await write.UpsertAsync(mobile);
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);
        var read = second.GetDataAccess<MobileEntity, Serial>();

        try
        {
            var loaded = await read.GetByIdAsync(mobileId);

            Assert.NotNull(loaded);
            Assert.Equal("Arthorius", loaded!.Name);
            Assert.Equal("the Brave", loaded.Title);
            Assert.Equal(new(7), loaded.AccountId);
            Assert.True(loaded.IsPlayer);
            Assert.Equal(GenderType.Male, loaded.Gender);
            Assert.Equal((Hue)0x83EA, loaded.SkinHue);
            Assert.Equal(100, loaded.BaseStats.Strength);
            Assert.Equal(25, loaded.Resistances.Energy);
            Assert.Equal(95, loaded.Resources.Hits);
            Assert.Equal(2, loaded.Skills.Count);
            Assert.Equal(99.5, loaded.Skills[UOSkillName.Swords].Value);
            Assert.Equal(UOSkillLock.Locked, loaded.Skills[UOSkillName.Tactics].Lock);
            Assert.Equal(new(Serial.ItemOffset + 2), loaded.EquippedItemIds[ItemLayerType.OneHanded]);
            Assert.Equal(new(Serial.ItemOffset + 1), loaded.BackpackId);
            Assert.Equal("seeded", loaded.CustomProperties["origin"].StringValue);
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
            new(new PersistenceEntityDescriptor<MobileEntity, Serial>(MobileEntityTypeId, "MobileEntity", 1, m => m.Id))
        };

        return new(_dir, config, registrations);
    }
}
