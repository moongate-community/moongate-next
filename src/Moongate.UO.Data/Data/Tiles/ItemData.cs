using System.Runtime.CompilerServices;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.UO.Data.Data.Tiles;

/// <summary>
/// Static properties of a single item/static tile, as read from <c>tiledata.mul</c>.
/// </summary>
public struct ItemData
{
    private byte _weight;
    private byte _quality;
    private ushort _animation;
    private byte _quantity;
    private byte _value;
    private byte _height;

    public ItemData(
        string name,
        UoTileFlag flags,
        int weight,
        int quality,
        int animation,
        int quantity,
        int value,
        int height
    )
    {
        Name = name;
        Flags = flags;
        _weight = (byte)weight;
        _quality = (byte)quality;
        _animation = (ushort)animation;
        _quantity = (byte)quantity;
        _value = (byte)value;
        _height = (byte)height;
    }

    public string Name { get; set; }

    public UoTileFlag Flags { get; set; }

    public int Weight
    {
        get => _weight;
        set => _weight = (byte)value;
    }

    public int Quality
    {
        get => _quality;
        set => _quality = (byte)value;
    }

    public int Animation
    {
        get => _animation;
        set => _animation = (ushort)value;
    }

    public int Quantity
    {
        get => _quantity;
        set => _quantity = (byte)value;
    }

    public int Value
    {
        get => _value;
        set => _value = (byte)value;
    }

    public int Height
    {
        get => _height;
        set => _height = (byte)value;
    }

    public int CalcHeight => Bridge ? _height / 2 : _height;

    public bool Door
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Door];
        set => this[UoTileFlag.Door] = value;
    }

    public bool Background
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Background];
        set => this[UoTileFlag.Background] = value;
    }

    public bool Bridge
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Bridge];
        set => this[UoTileFlag.Bridge] = value;
    }

    public bool Wall
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Wall];
        set => this[UoTileFlag.Wall] = value;
    }

    public bool Window
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Window];
        set => this[UoTileFlag.Window] = value;
    }

    public bool Weapon
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Weapon];
        set => this[UoTileFlag.Weapon] = value;
    }

    public bool Impassable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Impassable];
        set => this[UoTileFlag.Impassable] = value;
    }

    public bool Surface
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Surface];
        set => this[UoTileFlag.Surface] = value;
    }

    public bool Roof
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Roof];
        set => this[UoTileFlag.Roof] = value;
    }

    public bool LightSource
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.LightSource];
        set => this[UoTileFlag.LightSource] = value;
    }

    public bool Wet
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Wet];
        set => this[UoTileFlag.Wet] = value;
    }

    public bool Wearable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[UoTileFlag.Wearable];
        set => this[UoTileFlag.Wearable] = value;
    }

    public bool this[UoTileFlag flag]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Flags & flag) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }
    }

    public override string ToString()
        => $" {Name} ({Flags})";
}
