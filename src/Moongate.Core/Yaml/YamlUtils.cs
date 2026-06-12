using System.Globalization;
using Moongate.Core.Time;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Moongate.Core.Yaml;

/// <summary>
/// Provides YAML serialization helpers using Moongate's shared naming and scalar conventions.
/// </summary>
public static class YamlUtils
{
    private static readonly ISerializer Serializer = new SerializerBuilder()
                                                     .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                                     .WithTypeConverter(new TimeSpanYamlConverter())
                                                     .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                                                     .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
                                                         .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                                         .WithTypeConverter(new TimeSpanYamlConverter())
                                                         .WithTypeConverter(new SnakeCaseEnumYamlConverter())
                                                         .IgnoreUnmatchedProperties()
                                                         .Build();

    private sealed class TimeSpanYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
            => type == typeof(TimeSpan) || type == typeof(TimeSpan?);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();

            if (string.IsNullOrWhiteSpace(scalar.Value) && type == typeof(TimeSpan?))
            {
                return null;
            }

            return DurationParser.Parse(scalar.Value);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is null)
            {
                emitter.Emit(new Scalar("null"));

                return;
            }

            emitter.Emit(new Scalar(((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture)));
        }
    }

    private sealed class SnakeCaseEnumYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            var enumType = Nullable.GetUnderlyingType(type) ?? type;

            return enumType.IsEnum;
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();
            var enumType = Nullable.GetUnderlyingType(type) ?? type;

            if (string.IsNullOrWhiteSpace(scalar.Value) && Nullable.GetUnderlyingType(type) is not null)
            {
                return null;
            }

            if (Enum.TryParse(enumType, scalar.Value, true, out var parsed))
            {
                return parsed;
            }

            foreach (var name in Enum.GetNames(enumType))
            {
                if (string.Equals(ToSnakeCase(name), scalar.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return Enum.Parse(enumType, name);
                }
            }

            throw new YamlException($"Value '{scalar.Value}' is not valid for enum {enumType.Name}.");
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
            => serializer(value, type);

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var chars = new List<char>(value.Length + 4);

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];

                if (index > 0 && char.IsUpper(character))
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(character));
            }

            return new(chars.ToArray());
        }
    }

    /// <summary>Deserializes YAML text into the requested type.</summary>
    /// <param name="yaml">YAML text.</param>
    /// <typeparam name="T">Target type.</typeparam>
    /// <returns>The deserialized instance.</returns>
    public static T Deserialize<T>(string yaml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        return Deserializer.Deserialize<T>(yaml) ??
               throw new YamlException($"Deserialization returned null for type {typeof(T).Name}");
    }

    /// <summary>Deserializes YAML text into the requested runtime type.</summary>
    /// <param name="yaml">YAML text.</param>
    /// <param name="type">Target runtime type.</param>
    /// <returns>The deserialized instance.</returns>
    public static object Deserialize(string yaml, Type type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(type);

        return Deserializer.Deserialize(yaml, type) ??
               throw new YamlException($"Deserialization returned null for type {type.Name}");
    }

    /// <summary>Deserializes a YAML file into the requested type.</summary>
    /// <param name="filePath">YAML file path.</param>
    /// <typeparam name="T">Target type.</typeparam>
    /// <returns>The deserialized instance.</returns>
    public static T DeserializeFromFile<T>(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"The file '{normalizedPath}' does not exist.", normalizedPath);
        }

        return Deserialize<T>(File.ReadAllText(normalizedPath));
    }

    /// <summary>Serializes an instance to YAML.</summary>
    /// <param name="obj">Object to serialize.</param>
    /// <typeparam name="T">Source type.</typeparam>
    /// <returns>Serialized YAML text.</returns>
    public static string Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return Serializer.Serialize(obj);
    }

    /// <summary>Serializes an instance to YAML using a runtime type.</summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="type">Source runtime type.</param>
    /// <returns>Serialized YAML text.</returns>
    public static string Serialize(object obj, Type type)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(type);

        return Serializer.Serialize(obj, type);
    }

    /// <summary>Serializes an instance to a YAML file, creating parent directories as needed.</summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="filePath">Output YAML file path.</param>
    /// <typeparam name="T">Source type.</typeparam>
    public static void SerializeToFile<T>(T obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(normalizedPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(normalizedPath, Serialize(obj));
    }
}
