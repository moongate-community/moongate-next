using MessagePack;
using MessagePack.Formatters;
using Moongate.Core.Ids;

namespace Moongate.Persistence.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Serial" />, persisted as its underlying 32-bit value.
/// Contractless cannot round-trip <see cref="Serial" /> (get-only <c>Value</c>, no matching ctor
/// parameter name), so the descriptor's composite resolver registers this formatter first.
/// </summary>
public sealed class SerialMessagePackFormatter : IMessagePackFormatter<Serial>
{
    public static readonly SerialMessagePackFormatter Instance = new();

    public Serial Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        => new(reader.ReadUInt32());

    public void Serialize(ref MessagePackWriter writer, Serial value, MessagePackSerializerOptions options)
        => writer.Write(value.Value);
}
