using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.System;

/// <summary>
/// Represents a client hardware and installation information packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Spy On Client")]
public class SpyOnClientPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xD9;
    private const int LengthValue = 0x10C;

    public byte ClientInfoVersion { get; private set; }
    public uint InstanceId { get; private set; }
    public uint OsMajor { get; private set; }
    public uint OsMinor { get; private set; }
    public uint OsRevision { get; private set; }
    public byte CpuManufacturer { get; private set; }
    public uint CpuFamily { get; private set; }
    public uint CpuModel { get; private set; }
    public uint CpuClockSpeed { get; private set; }
    public byte CpuQuantity { get; private set; }
    public uint PhysicalMemory { get; private set; }
    public uint ScreenWidth { get; private set; }
    public uint ScreenHeight { get; private set; }
    public uint ScreenDepth { get; private set; }
    public ushort DirectXVersion { get; private set; }
    public ushort DirectXMinor { get; private set; }
    public string VideoCardDescription { get; private set; } = "";
    public uint VideoCardVendorId { get; private set; }
    public uint VideoCardDeviceId { get; private set; }
    public uint VideoCardMemory { get; private set; }
    public byte Distribution { get; private set; }
    public byte ClientsRunning { get; private set; }
    public byte ClientsInstalled { get; private set; }
    public byte PartialInstalled { get; private set; }
    public string LanguageCode { get; private set; } = "";
    public string UnknownEnding { get; private set; } = "";

    public SpyOnClientPacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != LengthValue - 1)
        {
            return false;
        }

        ClientInfoVersion = reader.ReadByte();
        InstanceId = reader.ReadUInt32();
        OsMajor = reader.ReadUInt32();
        OsMinor = reader.ReadUInt32();
        OsRevision = reader.ReadUInt32();
        CpuManufacturer = reader.ReadByte();
        CpuFamily = reader.ReadUInt32();
        CpuModel = reader.ReadUInt32();
        CpuClockSpeed = reader.ReadUInt32();
        CpuQuantity = reader.ReadByte();
        PhysicalMemory = reader.ReadUInt32();
        ScreenWidth = reader.ReadUInt32();
        ScreenHeight = reader.ReadUInt32();
        ScreenDepth = reader.ReadUInt32();
        DirectXVersion = reader.ReadUInt16();
        DirectXMinor = reader.ReadUInt16();
        VideoCardDescription = reader.ReadLittleUniSafe(64).TrimEnd();
        VideoCardVendorId = reader.ReadUInt32();
        VideoCardDeviceId = reader.ReadUInt32();
        VideoCardMemory = reader.ReadUInt32();
        Distribution = reader.ReadByte();
        ClientsRunning = reader.ReadByte();
        ClientsInstalled = reader.ReadByte();
        PartialInstalled = reader.ReadByte();
        LanguageCode = reader.ReadLittleUniSafe(4).TrimEnd();
        UnknownEnding = reader.ReadAsciiSafe(64).TrimEnd();

        return reader.Remaining == 0;
    }
}
