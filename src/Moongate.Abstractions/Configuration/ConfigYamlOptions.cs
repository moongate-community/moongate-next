using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Moongate.Abstractions.Configuration;

/// <summary>
/// Shared YAML serializer options for config serialization and deserialization.
/// </summary>
public static class ConfigYamlOptions
{
    public static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new TimeSpanYamlConverter())
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new TimeSpanYamlConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    private sealed class TimeSpanYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(TimeSpan) || type == typeof(TimeSpan?);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();

            if (string.IsNullOrWhiteSpace(scalar.Value) && type == typeof(TimeSpan?))
            {
                return null;
            }

            return TimeSpan.Parse(scalar.Value, CultureInfo.InvariantCulture);
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
}
