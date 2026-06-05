using System.Buffers.Binary;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Data.Packets;
using Moongate.Network.UO.Registry;
using Moongate.Network.UO.Types.Packets;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Network.Internal;

/// <summary>
/// Pure per-session byte-stream parser: accumulates incoming bytes into a pending buffer and
/// extracts complete UO packets, invoking a callback per parsed packet. No sockets or event bus,
/// so the full framing behaviour (fixed/variable length, partial buffering, unknown opcodes,
/// invalid lengths, parse failures, buffer overflow) is unit-testable in isolation.
/// </summary>
internal sealed class PacketParser
{
    private readonly ILogger _logger = Log.ForContext<PacketParser>();
    private readonly PacketRegistry _registry;
    private readonly int _maxPendingBufferBytes;
    private readonly int _maxDeclaredPacketLength;

    public PacketParser(PacketRegistry registry, int maxPendingBufferBytes, int maxDeclaredPacketLength)
    {
        _registry = registry;
        _maxPendingBufferBytes = maxPendingBufferBytes;
        _maxDeclaredPacketLength = maxDeclaredPacketLength;
    }

    /// <summary>
    /// Appends <paramref name="incoming" /> to <paramref name="pendingBytes" /> and extracts every
    /// complete packet, invoking <paramref name="onPacket" /> for each. Drops the buffer when it
    /// exceeds the pending limit, on unknown opcodes, or on invalid declared lengths.
    /// </summary>
    public void Append(
        List<byte> pendingBytes,
        byte[] incoming,
        NetworkParserSessionMetrics metrics,
        Action<byte, IGameNetworkPacket, byte[]> onPacket
    )
    {
        metrics.AddReceivedBytes(incoming.Length);
        pendingBytes.AddRange(incoming);

        if (pendingBytes.Count > _maxPendingBufferBytes)
        {
            metrics.IncrementPendingBufferOverflows();
            _logger.Warning("Pending buffer limit exceeded ({Count} bytes); clearing buffer", pendingBytes.Count);
            pendingBytes.Clear();

            return;
        }

        ParseAvailable(pendingBytes, metrics, onPacket);
    }

    private void ParseAvailable(
        List<byte> pendingBytes,
        NetworkParserSessionMetrics metrics,
        Action<byte, IGameNetworkPacket, byte[]> onPacket
    )
    {
        while (pendingBytes.Count > 0)
        {
            var opCode = pendingBytes[0];

            if (!_registry.TryGetDescriptor(opCode, out var descriptor))
            {
                // Unknown opcode: we cannot know the length, so drop the whole buffer to resync.
                metrics.IncrementUnknownOpcodeDrops();
                _logger.Warning(
                    "Unknown opcode 0x{OpCode:X2}; dropping {Count} buffered bytes",
                    opCode,
                    pendingBytes.Count
                );
                pendingBytes.Clear();

                return;
            }

            var length = ResolvePacketLength(pendingBytes, descriptor);

            if (length is null)
            {
                // Need more bytes to determine the length.
                return;
            }

            if (length.Value <= 0 || length.Value > _maxDeclaredPacketLength)
            {
                metrics.IncrementInvalidLengthDrops();
                _logger.Warning(
                    "Invalid declared length {Length} for opcode 0x{OpCode:X2}; dropping buffer",
                    length.Value,
                    opCode
                );
                pendingBytes.Clear();

                return;
            }

            if (pendingBytes.Count < length.Value)
            {
                // Full packet not yet available.
                return;
            }

            var rawPacket = new byte[length.Value];
            pendingBytes.CopyTo(0, rawPacket, 0, length.Value);
            pendingBytes.RemoveRange(0, length.Value);

            if (!_registry.TryCreatePacket(opCode, out var packet) || packet is null)
            {
                metrics.IncrementUnknownOpcodeDrops();

                continue;
            }

            if (!packet.TryParse(rawPacket))
            {
                metrics.IncrementParseFailures();
                _logger.Warning("Failed to parse packet 0x{OpCode:X2}", opCode);

                continue;
            }

            metrics.IncrementParsedPackets();
            onPacket(opCode, packet, rawPacket);
        }
    }

    private static int? ResolvePacketLength(List<byte> pendingBytes, PacketDescriptor descriptor)
    {
        if (descriptor.Sizing == PacketSizing.Fixed)
        {
            return descriptor.Length;
        }

        if (pendingBytes.Count < 3)
        {
            return null;
        }

        Span<byte> lengthBuffer = [pendingBytes[1], pendingBytes[2]];

        return BinaryPrimitives.ReadUInt16BigEndian(lengthBuffer);
    }
}
