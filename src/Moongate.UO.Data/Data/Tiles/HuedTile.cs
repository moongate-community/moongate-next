using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Data.Tiles;

/// <summary>
/// A hued land tile variant used by map overlays: id, hue and z.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HuedTile
{
    internal sbyte m_Z;
    internal ushort m_ID;
    internal int m_Hue;

    public HuedTile(ushort id, short hue, sbyte z)
    {
        m_ID = id;
        m_Hue = hue;
        m_Z = z;
    }

    public ushort ID
    {
        get => m_ID;
        set => m_ID = value;
    }

    public int Hue
    {
        get => m_Hue;
        set => m_Hue = value;
    }

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public void Set(ushort id, short hue, sbyte z)
    {
        m_ID = id;
        m_Hue = hue;
        m_Z = z;
    }
}
