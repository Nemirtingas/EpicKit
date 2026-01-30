using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementTier
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "hexColor")]
    public string HexColor { get; set; }

    [JsonProperty(PropertyName = "min")]
    public long Min { get; set; }

    [JsonProperty(PropertyName = "max")]
    public long Max { get; set; }

    public StoreApplicationAchievementTier()
    {
        Reset();
    }

    public void Reset()
    {
        Name = string.Empty;
        HexColor = string.Empty;
        Min = 0;
        Max = 0;
    }
}