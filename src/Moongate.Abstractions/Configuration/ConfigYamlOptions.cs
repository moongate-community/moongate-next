using System.Globalization;
using Moongate.Core.Geometry;
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
                                                    .WithTypeConverter(new Point3DYamlConverter())
                                                    .WithTypeConverter(new TimeSpanYamlConverter())
                                                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                                                    .Build();

    public static readonly IDeserializer Deserializer = new DeserializerBuilder()
                                                        .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                                        .WithTypeConverter(new Point3DYamlConverter())
                                                        .WithTypeConverter(new TimeSpanYamlConverter())
                                                        .IgnoreUnmatchedProperties()
                                                        .Build();

    private sealed class Point3DYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
            => type == typeof(Point3D) || Nullable.GetUnderlyingType(type) == typeof(Point3D);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var scalar = parser.Consume<Scalar>();

            if (string.IsNullOrWhiteSpace(scalar.Value) && Nullable.GetUnderlyingType(type) == typeof(Point3D))
            {
                return null;
            }

            return ParsePoint3D(scalar.Value);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is null)
            {
                emitter.Emit(new Scalar("null"));

                return;
            }

            var point = (Point3D)value;
            emitter.Emit(new Scalar(FormattableString.Invariant($"{point.X},{point.Y},{point.Z}")));
        }

        private static Point3D ParsePoint3D(string value)
        {
            if (Point3D.TryParse(value, CultureInfo.InvariantCulture, out var point))
            {
                return point;
            }

            var parts = value.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length != 3 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var z))
            {
                throw new FormatException($"Point3D value '{value}' must use 'x,y,z' format.");
            }

            return new(x, y, z);
        }
    }

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

    /// <summary>
    /// Maps a CLR property name to the config key used in YAML and in dotted config paths
    /// (e.g. <c>UseSsl</c> → <c>use_ssl</c>), using the same naming convention as the serializer.
    /// </summary>
    public static string ToConfigKey(string name)
        => UnderscoredNamingConvention.Instance.Apply(name);
}
