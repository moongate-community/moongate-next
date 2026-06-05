using System.Globalization;
using System.Text.Json;
using Tomlyn;
using Tomlyn.Serialization;

namespace Moongate.Abstractions.Configuration;

/// <summary>
/// Shared Tomlyn serializer options for config (de)serialization. Enables Tomlyn's reflection-based
/// (de)serialization, which is disabled by default for trimming/AOT, so
/// plain POCO configs bind without source-generated contexts.
/// </summary>
public static class ConfigTomlOptions
{
    static ConfigTomlOptions()
    {
        AppContext.SetSwitch("Tomlyn.TomlSerializer.IsReflectionEnabledByDefault", true);
    }

    public static readonly TomlSerializerOptions Instance = new()
    {
        Converters = [new TimeSpanStringConverter()],
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private sealed class TimeSpanStringConverter : TomlConverter<TimeSpan>
    {
        public override TimeSpan Read(TomlReader reader)
        {
            var value = reader.GetString();
            reader.Read();

            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void Write(TomlWriter writer, TimeSpan value)
            => writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
    }
}
