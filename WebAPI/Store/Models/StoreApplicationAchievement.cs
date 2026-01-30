using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

public class StoreApplicationAchievement
{
    [JsonProperty(PropertyName = "sandboxId")]
    public string SandboxId { get; set; }

    [JsonProperty(PropertyName = "deploymentId")]
    public string DeploymentId { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "hidden")]
    public bool Hidden { get; set; }

    [JsonProperty(PropertyName = "isBase")]
    public bool IsBase { get; set; }

    [JsonProperty(PropertyName = "achievementSetId")]
    public string AchievementSetId { get; set; }

    [JsonProperty(PropertyName = "unlockedDisplayName")]
    public string UnlockedDisplayName { get; set; }

    [JsonProperty(PropertyName = "lockedDisplayName")]
    public string LockedDisplayName { get; set; }

    [JsonProperty(PropertyName = "unlockedDescription")]
    public string UnlockedDescription { get; set; }

    [JsonProperty(PropertyName = "lockedDescription")]
    public string LockedDescription { get; set; }

    [JsonProperty(PropertyName = "unlockedIconId")]
    public string UnlockedIconId { get; set; }

    [JsonProperty(PropertyName = "lockedIconId")]
    public string LockedIconId { get; set; }

    [JsonProperty(PropertyName = "XP")]
    public long XP { get; set; }

    [JsonProperty(PropertyName = "flavorText")]
    public string FlavorText { get; set; }

    [JsonProperty(PropertyName = "unlockedIconLink")]
    public string UnlockedIconLink { get; set; }

    [JsonProperty(PropertyName = "lockedIconLink")]
    public string LockedIconLink { get; set; }

    [JsonProperty(PropertyName = "tier")]
    public StoreApplicationAchievementTier Tier { get; set; }

    [JsonProperty(PropertyName = "rarity")]
    public StoreApplicationAchievementRarity Rarity { get; set; }


    public StoreApplicationAchievement()
    {
        Reset();
    }

    public void Reset()
    {
        SandboxId = string.Empty;
        DeploymentId = string.Empty;
        Name = string.Empty;
        Hidden = false;
        IsBase = false;
        AchievementSetId = string.Empty;
        UnlockedDisplayName = string.Empty;
        LockedDisplayName = string.Empty;
        UnlockedDescription = string.Empty;
        LockedDescription = string.Empty;
        UnlockedIconId = string.Empty;
        LockedIconId = string.Empty;
        XP = 0;
        FlavorText = string.Empty;
        UnlockedIconLink = string.Empty;
        LockedIconLink = string.Empty;
        Tier = new();
        Rarity = new();
    }
}