using Moongate.Network.Spans;

namespace Moongate.Network.UO.Data.Login;

public sealed class CityInfo
{
    public CityInfo(string city, string building, int x, int y, int z, int mapIndex = 0, int description = 0)
    {
        City = city;
        Building = building;
        X = x;
        Y = y;
        Z = z;
        MapIndex = mapIndex;
        Description = description;
    }

    public static int Length => 89;

    public string City { get; set; } = "";

    public string Building { get; set; } = "";

    public int Description { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int MapIndex { get; set; }

    public byte[] ToArray(int index)
    {
        using var writer = new SpanWriter(stackalloc byte[Length], true);

        writer.Write((byte)index);
        writer.WriteAscii(City, 32);
        writer.WriteAscii(Building, 32);
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Z);
        writer.Write(MapIndex);
        writer.Write(Description);
        writer.Write(0);

        return writer.Span.ToArray();
    }
}
