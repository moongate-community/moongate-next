using MessagePack;
using MessagePack.Formatters;
using Moongate.Core.Ids;

namespace Moongate.Persistence.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="AutoInt64" />, persisted as its underlying 64-bit signed value.
/// </summary>
public sealed class AutoInt64MessagePackFormatter : IMessagePackFormatter<AutoInt64>
{
    public static readonly AutoInt64MessagePackFormatter Instance = new();

    public AutoInt64 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        => new(reader.ReadInt64());

    public void Serialize(ref MessagePackWriter writer, AutoInt64 value, MessagePackSerializerOptions options)
        => writer.Write(value.Value);
}
