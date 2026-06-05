using MessagePack;
using MessagePack.Formatters;
using Moongate.Core.Ids;

namespace Moongate.Persistence.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="AutoInt32" />, persisted as its underlying 32-bit signed value.
/// </summary>
public sealed class AutoInt32MessagePackFormatter : IMessagePackFormatter<AutoInt32>
{
    public static readonly AutoInt32MessagePackFormatter Instance = new();

    public AutoInt32 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        => new(reader.ReadInt32());

    public void Serialize(ref MessagePackWriter writer, AutoInt32 value, MessagePackSerializerOptions options)
        => writer.Write(value.Value);
}
