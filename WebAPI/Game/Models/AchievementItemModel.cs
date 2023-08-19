using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class AchievementItemV2Model
    {
        [JsonProperty("achievement")]
        public AchievementModel Achievement { get; set; }
        [JsonProperty("iconLinks")]
        public Dictionary<string, AchievementIconLinksModel> IconLinks { get; set; }
    }
}
