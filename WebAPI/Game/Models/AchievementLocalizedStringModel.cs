using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class AchievementMappedStringModel
    {
        [JsonProperty("requested")]
        public string Requested { get; set; }
        [JsonProperty("found")]
        public string Found { get; set; }
    }

    public class AchievementLocalizedStringModel
    {
        [JsonProperty("default")]
        public string Default { get; set; }
        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }
        [JsonProperty("mapped")]
        public AchievementMappedStringModel Mapped { get; set; }

        public string GetLocalizedString(string locale, out bool isDefault)
        {
            if (Data.ContainsKey(locale))
            {
                isDefault = false;
                return Data[locale];
            }

            isDefault = true;
            return Default;
        }
    }
}
