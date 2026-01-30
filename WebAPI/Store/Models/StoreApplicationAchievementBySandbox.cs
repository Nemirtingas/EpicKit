using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievementBySandbox
{
    [JsonProperty(PropertyName = "productId")]
    public string ProductId { get; set; }

    [JsonProperty(PropertyName = "sandboxId")]
    public string SandboxId { get; set; }

    [JsonProperty(PropertyName = "totalAchievements")]
    public long TotalAchievements { get; set; }

    [JsonProperty(PropertyName = "totalProductXP")]
    public long TotalProductXP { get; set; }

    [JsonProperty(PropertyName = "achievementSets")]
    public List<StoreApplicationAchievementSet> AchievementSets { get; set; }

    [JsonProperty(PropertyName = "platinumRarity")]
    public StoreApplicationAchievementPlatinumRarity PlatinumRarity { get; set; }

    [JsonProperty(PropertyName = "achievements")]
    public List<StoreApplicationAchievementWrapper> Achievements { get; set; }

    public StoreApplicationAchievementBySandbox()
    {
        Reset();
    }

    public void Reset()
    {
        ProductId = string.Empty;
        SandboxId = string.Empty;
        TotalAchievements = 0;
        TotalProductXP = 0;
        AchievementSets = new();
        PlatinumRarity = new();
        Achievements = new();
    }
}