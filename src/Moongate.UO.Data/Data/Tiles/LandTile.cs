using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Data.Tiles;

/// <summary>
/// A single land tile read from a map facet block: tile id and z. 3 bytes, packed for direct reads.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LandTile
{
    internal short m_ID;
    internal sbyte m_Z;

    public LandTile(short id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }

    public int ID => m_ID;

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public int Height => 0;

    public bool Ignored => m_ID is 2 or 0x1DB or (>= 0x1AE and <= 0x1B5);

    public void Set(short id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }
}
