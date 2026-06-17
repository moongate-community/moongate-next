using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.Spans;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using Xunit;

namespace Moongate.Tests.Network.Packets.Entity;

public sealed class DrawPacketsTests
{
    private static byte[] Serialize(Moongate.Network.UO.Base.BaseGameNetworkPacket packet)
    {
        var buffer = new byte[512];
        var writer = new SpanWriter(buffer);
        packet.Write(ref writer);
        return buffer[..writer.Position].ToArray();
    }

    [Fact]
    public void DeleteObject_Writes_Opcode_And_Serial()
    {
        var bytes = Serialize(new DeleteObjectPacket(new Serial(0x1234)));
        Assert.Equal(5, bytes.Length);
        Assert.Equal(0x1D, bytes[0]);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x12, 0x34 }, bytes[1..5]);
    }

    [Fact]
    public void MobileMoving_Is17Bytes_WithOpcodeAndSerialAndLocation()
    {
        var m = new MobileEntity { Id = new Serial(0x40A), BodyId = 0x190, Location = new Point3D(100, 200, 5), Direction = DirectionType.South, SkinHue = (Hue)0x83EA, Notoriety = NotorietyType.Innocent };
        var bytes = Serialize(new MobileMovingPacket(m));
        Assert.Equal(17, bytes.Length);
        Assert.Equal(0x77, bytes[0]);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x04, 0x0A }, bytes[1..5]);
        Assert.Equal(new byte[] { 0x01, 0x90 }, bytes[5..7]);
        Assert.Equal((byte)NotorietyType.Innocent, bytes[16]);
    }

    [Fact]
    public void ObjectInformation_Is26Bytes_WithGraphicAndLocation()
    {
        var item = new ItemEntity { Id = new Serial(0x40000001), ItemId = 0x0EED, Amount = 7, Location = new Point3D(123, 456, -3), Hue = (Hue)0x0021 };
        var bytes = Serialize(new ObjectInformationPacket(item));
        Assert.Equal(26, bytes.Length);
        Assert.Equal(0xF3, bytes[0]);
        Assert.Equal(new byte[] { 0x00, 0x01 }, bytes[1..3]);
        Assert.Equal(new byte[] { 0x0E, 0xED }, bytes[8..10]);
    }

    [Fact]
    public void MobileIncoming_StartsWithOpcodeAndLength_AndCarriesSerial()
    {
        var m = new MobileEntity { Id = new Serial(0x40B), BodyId = 0x190, Location = new Point3D(10, 20, 0), Direction = DirectionType.North, SkinHue = (Hue)0, Notoriety = NotorietyType.Innocent };
        var bytes = Serialize(new MobileIncomingPacket(m, []));
        Assert.Equal(0x78, bytes[0]);
        var declaredLength = (bytes[1] << 8) | bytes[2];
        Assert.Equal(bytes.Length, declaredLength);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x04, 0x0B }, bytes[3..7]);
    }

    [Fact]
    public void MobileIncoming_WithEquippedItem_IsLongerThanBareBody()
    {
        var m = new MobileEntity { Id = new Serial(0x40C), BodyId = 0x190, Location = new Point3D(10, 20, 0), Direction = DirectionType.North, SkinHue = (Hue)0, Notoriety = NotorietyType.Innocent };
        var bare = Serialize(new MobileIncomingPacket(m, []));
        var sword = new ItemEntity { Id = new Serial(0x40000005), ItemId = 0x0F5E, Hue = (Hue)0x1B };
        var clothed = Serialize(new MobileIncomingPacket(m, [(ItemLayerType.OneHanded, sword)]));
        Assert.True(clothed.Length > bare.Length);
    }
}
