namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetWeatherTable
{
    public ServerAssetWeatherHeader Header { get; set; } = new();
    public List<ServerAssetWeatherDefinition> WeatherType { get; set; } = [];
}
