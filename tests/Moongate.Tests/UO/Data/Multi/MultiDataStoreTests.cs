using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Multi;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.UO.Data.Multi;

public class MultiDataStoreTests
{
    [Fact]
    public void EmptyDirectory_YieldsEmptyStore()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new MultiDataStore(new UoFileResolver(dir.FullName));

            Assert.Equal(0, store.Count);
            Assert.Empty(store.GetComponents(1).List);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MulPath_LoadsComponents_AndUnknownIdReturnsEmpty()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            MultiFixture.WriteMul(
                dir.FullName,
                new Dictionary<int, IReadOnlyList<MultiFixture.Tile>>
                {
                    [0] = [],
                    [1] = [new MultiFixture.Tile(0x100, 0, 0, 0, 1), new MultiFixture.Tile(0x101, 1, 0, 0, 1)]
                }
            );
            var store = new MultiDataStore(new UoFileResolver(dir.FullName));

            var house = store.GetComponents(1);

            Assert.Equal(2, house.List.Length);
            Assert.Equal(2, house.Width);
            Assert.Empty(store.GetComponents(9999).List);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void ParseUopEntry_ReadsTiles()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(0);            // 4 skipped bytes
        bw.Write((uint)1);      // tile count
        bw.Write((ushort)0x55); // itemId
        bw.Write((short)2);     // x
        bw.Write((short)3);     // y
        bw.Write((short)4);     // z
        bw.Write((ushort)257);  // flagValue -> Generic
        bw.Write((uint)0);      // clilocsCount
        bw.Flush();

        var tiles = MultiDataStore.ParseUopEntry(ms.ToArray());

        Assert.Single(tiles);
        Assert.Equal(0x55, tiles[0].ItemId);
        Assert.Equal(2, tiles[0].OffsetX);
        Assert.Equal(UoTileFlag.Generic, tiles[0].Flags);
    }
}
