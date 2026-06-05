using Moongate.UO.Data.Data.Multi;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.UO.Data.Multi;

public class MultiComponentListTests
{
    [Fact]
    public void ClassicCtor_ParsesDimensionsTilesAndList()
    {
        var bytes = BuildMulEntries(
            [
                (0x100, 0, 0, 0, 1),
                (0x101, 1, 0, 5, 1)
            ]
        );
        using var reader = new BinaryReader(new MemoryStream(bytes));

        var mcl = new MultiComponentList(reader, bytes.Length, true);

        Assert.Equal(2, mcl.Width);
        Assert.Equal(1, mcl.Height);
        Assert.Equal(2, mcl.List.Length);
        Assert.Equal(0, mcl.Center.X);
        Assert.Equal(0x100, mcl.Tiles[0][0][0].ID);
        Assert.Equal(0x101, mcl.Tiles[1][0][0].ID);
        Assert.Equal(5, mcl.Tiles[1][0][0].Z);
    }

    [Fact]
    public void Empty_HasNoTilesOrList()
    {
        Assert.Empty(MultiComponentList.Empty.List);
        Assert.Empty(MultiComponentList.Empty.Tiles);
    }

    [Fact]
    public void FromListCtor_BuildsEquivalentGeometry()
    {
        var list = new List<MultiTileEntry>
        {
            new(0x200, 0, 0, 0, UoTileFlag.Background),
            new(0x201, 0, 1, 0, UoTileFlag.Background)
        };

        var mcl = new MultiComponentList(list);

        Assert.Equal(1, mcl.Width);
        Assert.Equal(2, mcl.Height);
        Assert.Equal(0x201, mcl.Tiles[0][1][0].ID);
    }

    private static byte[] BuildMulEntries((ushort id, short x, short y, short z, ulong flags)[] tiles)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        foreach (var t in tiles)
        {
            bw.Write(t.id);
            bw.Write(t.x);
            bw.Write(t.y);
            bw.Write(t.z);
            bw.Write(t.flags);
        }

        bw.Flush();

        return ms.ToArray();
    }
}
