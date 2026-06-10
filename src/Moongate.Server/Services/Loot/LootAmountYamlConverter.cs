using System.Globalization;
using Moongate.UO.Data.Templates.Loot;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Events = YamlDotNet.Core.Events;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Reads the loot <c>amount</c> field in either form: a scalar (<c>amount: 5</c>,
/// fixed) or a mapping (<c>amount: { min: 1, max: 100 }</c>, range).
/// </summary>
public sealed class LootAmountYamlConverter : IYamlTypeConverter
{
    private sealed class LootAmountRaw
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }

    public bool Accepts(Type type)
        => type == typeof(LootAmount);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<Events.Scalar>(out var scalar))
        {
            var value = int.Parse(scalar.Value, CultureInfo.InvariantCulture);

            return new LootAmount(value, value);
        }

        var raw = (LootAmountRaw?)rootDeserializer(typeof(LootAmountRaw));
        var min = raw?.Min ?? 0;
        var max = raw?.Max ?? min;

        return new LootAmount(min, max);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var amount = (LootAmount?)value ?? new LootAmount(0, 0);

        emitter.Emit(new Events.MappingStart());
        emitter.Emit(new Events.Scalar("min"));
        emitter.Emit(new Events.Scalar(amount.Min.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Events.Scalar("max"));
        emitter.Emit(new Events.Scalar(amount.Max.ToString(CultureInfo.InvariantCulture)));
        emitter.Emit(new Events.MappingEnd());
    }
}
