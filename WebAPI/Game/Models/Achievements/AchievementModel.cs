using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Achievements
{
    public class AchievementModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("unlockedDisplayName")]
        public AchievementLocalizedStringModel UnlockedDisplayName { get; set; }
        [JsonProperty("unlockedDescription")]
        public AchievementLocalizedStringModel UnlockedDescription { get; set; }
        [JsonProperty("lockedDisplayName")]
        public AchievementLocalizedStringModel LockedDisplayName { get; set; }
        [JsonProperty("lockedDescription")]
        public AchievementLocalizedStringModel LockedDescription { get; set; }
        [JsonProperty("hidden")]
        public bool Hidden { get; set; }
        [JsonProperty("unlockedIconId")]
        public AchievementLocalizedStringModel UnlockedIconId { get; set; }
        [JsonProperty("lockedIconId")]
        public AchievementLocalizedStringModel LockedIconId { get; set; }
        [JsonProperty("statThresholds")]
        public Dictionary<string, long> StatThresholds { get; set; }
        [JsonProperty("flavorText")]
        public AchievementLocalizedStringModel FlavorText { get; set; }
    }
}
