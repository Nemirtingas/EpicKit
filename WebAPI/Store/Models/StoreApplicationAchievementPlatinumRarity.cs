using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementPlatinumRarity
{
    [JsonProperty(PropertyName = "percent")]
    public decimal Percent { get; set; }

    public StoreApplicationAchievementPlatinumRarity()
    {
        Reset();
    }

    public void Reset()
    {
        Percent = 0m;
    }
}