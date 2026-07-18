using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievement
{
    [JsonProperty(PropertyName = "achievement")]
    public StoreApplicationAchievementDefinition Achievement { get; set; }

    public StoreApplicationAchievement()
    {
        Reset();
    }

    public void Reset()
    {
        Achievement = new();
    }
}