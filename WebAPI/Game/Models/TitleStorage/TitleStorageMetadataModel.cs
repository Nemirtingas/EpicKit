using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.TitleStorage
{
    public class TitleStorageMetadataModel
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
