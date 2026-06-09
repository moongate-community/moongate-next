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
    // The 0xEF login-seed packet (21 bytes, seed + version) opens a login-server connection; any other
    // first byte on a fresh connection is a raw 4-byte seed (game-server reconnect).
    private const byte LoginSeedOpCode = 0xEF;

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
        Action<byte, IGameNetworkPacket, byte[]> onPacket,
        PacketStreamState? state = null
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

        ParseAvailable(pendingBytes, metrics, onPacket, state);
    }

    private void ParseAvailable(
        List<byte> pendingBytes,
        NetworkParserSessionMetrics metrics,
        Action<byte, IGameNetworkPacket, byte[]> onPacket,
        PacketStreamState? state
    )
    {
        while (pendingBytes.Count > 0)
        {
            if (state is { SeedConsumed: false })
            {
                // Consume the initial seed before any packet framing. Returns false when more bytes
                // are still needed (a partial raw seed).
                if (!TryConsumeSeed(pendingBytes, state))
                {
                    return;
                }

                continue;
            }

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

    private static bool TryConsumeSeed(List<byte> pendingBytes, PacketStreamState state)
    {
        if (pendingBytes[0] == LoginSeedOpCode)
        {
            // Login connection: the 0xEF packet carries the seed; let normal framing parse it.
            state.SeedConsumed = true;

            return true;
        }

        if (pendingBytes.Count < 4)
        {
            // Wait for the full raw 4-byte seed.
            return false;
        }

        Span<byte> seedBytes = [pendingBytes[0], pendingBytes[1], pendingBytes[2], pendingBytes[3]];
        state.Seed = BinaryPrimitives.ReadUInt32BigEndian(seedBytes);
        state.SeedConsumed = true;
        pendingBytes.RemoveRange(0, 4);

        return true;
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
