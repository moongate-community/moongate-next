using MessagePack;
using MessagePack.Formatters;

namespace Moongate.Persistence.Formatters;

/// <summary>
/// MessagePack formatter for <see cref="Dictionary{TKey,TValue}" /> with string keys that
/// reconstructs the dictionary with <see cref="StringComparer.OrdinalIgnoreCase" /> on
/// deserialize. The contractless resolver otherwise rebuilds string-keyed dictionaries with the
/// default ordinal comparer, silently dropping case-insensitive lookups across a round-trip.
/// </summary>
public sealed class CaseInsensitiveStringDictionaryFormatter<TValue> : IMessagePackFormatter<Dictionary<string, TValue>?>
{
    public Dictionary<string, TValue>? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var valueFormatter = options.Resolver.GetFormatterWithVerify<TValue>();
        var count = reader.ReadMapHeader();
        var result = new Dictionary<string, TValue>(count, StringComparer.OrdinalIgnoreCase);

        options.Security.DepthStep(ref reader);

        try
        {
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString() ?? "";
                result[key] = valueFormatter.Deserialize(ref reader, options);
            }
        }
        finally
        {
            reader.Depth--;
        }

        return result;
    }

    public void Serialize(
        ref MessagePackWriter writer,
        Dictionary<string, TValue>? value,
        MessagePackSerializerOptions options
    )
    {
        if (value is null)
        {
            writer.WriteNil();

            return;
        }

        var valueFormatter = options.Resolver.GetFormatterWithVerify<TValue>();

        writer.WriteMapHeader(value.Count);

        foreach (var pair in value)
        {
            writer.Write(pair.Key);
            valueFormatter.Serialize(ref writer, pair.Value, options);
        }
    }
}
