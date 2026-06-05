using Moongate.UO.Data.Types.Tiles;

namespace Moongate.UO.Data.Data.Multi;

/// <summary>
/// One component of a multi: an item id placed at an offset from the multi's centre, with flags.
/// </summary>
public struct MultiTileEntry
{
    public ushort ItemId { get; set; }
    public short OffsetX { get; set; }
    public short OffsetY { get; set; }
    public short OffsetZ { get; set; }
    public UoTileFlag Flags { get; set; }

    public MultiTileEntry(ushort itemId, short offsetX, short offsetY, short offsetZ, UoTileFlag flags)
    {
        ItemId = itemId;
        OffsetX = offsetX;
        OffsetY = offsetY;
        OffsetZ = offsetZ;
        Flags = flags;
    }
}
