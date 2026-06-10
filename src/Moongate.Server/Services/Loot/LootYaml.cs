using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Shared YAML deserializer for loot tables: the project's underscored naming
/// convention plus the <see cref="LootAmountYamlConverter" /> for the amount shorthand.
/// </summary>
public static class LootYaml
{
    public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new LootAmountYamlConverter())
        .IgnoreUnmatchedProperties()
        .Build();
}
