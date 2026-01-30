using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementSet
{
    [JsonProperty(PropertyName = "achievementSetId")]
    public string AchievementSetId { get; set; }

    [JsonProperty(PropertyName = "isBase")]
    public bool IsBase { get; set; }

    [JsonProperty(PropertyName = "totalAchievements")]
    public long TotalAchievements { get; set; }

    [JsonProperty(PropertyName = "totalXP")]
    public long TotalXP { get; set; }

    public StoreApplicationAchievementSet()
    {
        Reset();
    }

    public void Reset()
    {
        AchievementSetId = string.Empty;
        IsBase = false;
        TotalAchievements = 0;
        TotalXP = 0;
    }
}