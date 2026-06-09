using MessagePack;
using MessagePack.Formatters;

namespace Moongate.Persistence.Formatters;

/// <summary>
/// Resolver that supplies <see cref="CaseInsensitiveStringDictionaryFormatter{TValue}" /> for any
/// <see cref="Dictionary{TKey,TValue}" /> whose key is <see cref="string" />, so persisted
/// string-keyed dictionaries keep case-insensitive lookups after a round-trip. Other types fall
/// through to the next resolver in the composite.
/// </summary>
public sealed class CaseInsensitiveStringDictionaryResolver : IFormatterResolver
{
    public static readonly CaseInsensitiveStringDictionaryResolver Instance = new();

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            var type = typeof(T);

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return;
            }

            var arguments = type.GetGenericArguments();

            if (arguments[0] != typeof(string))
            {
                return;
            }

            var formatterType = typeof(CaseInsensitiveStringDictionaryFormatter<>).MakeGenericType(arguments[1]);
            Formatter = (IMessagePackFormatter<T>?)Activator.CreateInstance(formatterType);
        }
    }

    public IMessagePackFormatter<T>? GetFormatter<T>()
        => FormatterCache<T>.Formatter;
}
