namespace Moongate.UO.Data.Data.ServerAssets;

public sealed class ServerAssetWeatherDefinition
{
    public int Id { get; set; }
    public int Rainchance { get; set; }
    public ServerAssetRange Rainintensity { get; set; } = new();
    public int Raintempdrop { get; set; }
    public int Snowchance { get; set; }
    public ServerAssetRange Snowintensity { get; set; } = new();
    public int Snowthreshold { get; set; }
    public int Stormchance { get; set; }
    public ServerAssetRange Stormintensity { get; set; } = new();
    public int Stormtempdrop { get; set; }
    public int Maxtemp { get; set; }
    public int Mintemp { get; set; }
    public int Coldchance { get; set; }
    public int Coldintensity { get; set; }
    public int Heatchance { get; set; }
    public int Heatintensity { get; set; }
    public int? Lightmin { get; set; }
    public int? Lightmax { get; set; }
    public string Name { get; set; } = "";
}
