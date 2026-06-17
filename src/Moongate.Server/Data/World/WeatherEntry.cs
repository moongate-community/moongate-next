namespace Moongate.Server.Data.World;

/// <summary>
///     Represents one weather definition loaded from server asset data.
/// </summary>
public readonly record struct WeatherEntry
{
    public WeatherEntry(
        int id,
        string name,
        int rainChance,
        WeatherRange rainIntensity,
        int rainTemperatureDrop,
        int snowChance,
        WeatherRange snowIntensity,
        int snowThreshold,
        int stormChance,
        WeatherRange stormIntensity,
        int stormTemperatureDrop,
        int maxTemperature,
        int minTemperature,
        int coldChance,
        int coldIntensity,
        int heatChance,
        int heatIntensity,
        int? lightMin,
        int? lightMax
    )
    {
        Id = id;
        Name = name;
        RainChance = rainChance;
        RainIntensity = rainIntensity;
        RainTemperatureDrop = rainTemperatureDrop;
        SnowChance = snowChance;
        SnowIntensity = snowIntensity;
        SnowThreshold = snowThreshold;
        StormChance = stormChance;
        StormIntensity = stormIntensity;
        StormTemperatureDrop = stormTemperatureDrop;
        MaxTemperature = maxTemperature;
        MinTemperature = minTemperature;
        ColdChance = coldChance;
        ColdIntensity = coldIntensity;
        HeatChance = heatChance;
        HeatIntensity = heatIntensity;
        LightMin = lightMin;
        LightMax = lightMax;
    }

    public int Id { get; }

    public string Name { get; }

    public int RainChance { get; }

    public WeatherRange RainIntensity { get; }

    public int RainTemperatureDrop { get; }

    public int SnowChance { get; }

    public WeatherRange SnowIntensity { get; }

    public int SnowThreshold { get; }

    public int StormChance { get; }

    public WeatherRange StormIntensity { get; }

    public int StormTemperatureDrop { get; }

    public int MaxTemperature { get; }

    public int MinTemperature { get; }

    public int ColdChance { get; }

    public int ColdIntensity { get; }

    public int HeatChance { get; }

    public int HeatIntensity { get; }

    public int? LightMin { get; }

    public int? LightMax { get; }
}
