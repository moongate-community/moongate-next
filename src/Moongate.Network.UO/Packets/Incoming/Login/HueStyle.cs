namespace Moongate.Network.UO.Packets.Incoming.Login;

/// <summary>
/// Hue/style pair selected during character login.
/// </summary>
public readonly record struct HueStyle
{
    public short Style { get; }
    public short Hue { get; }

    public HueStyle(short style, short hue)
    {
        Style = style;
        Hue = hue;
    }
}
