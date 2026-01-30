using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementRarity
{
    [JsonProperty(PropertyName = "percent")]
    public decimal Percent { get; set; }

    public StoreApplicationAchievementRarity()
    {
        Reset();
    }

    public void Reset()
    {
        Percent = 0m;
    }
}