using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementWrapper
{
    [JsonProperty(PropertyName = "achievement")]
    public StoreApplicationAchievement Achievement { get; set; }

    public StoreApplicationAchievementWrapper()
    {
        Reset();
    }

    public void Reset()
    {
        Achievement = new();
    }
}