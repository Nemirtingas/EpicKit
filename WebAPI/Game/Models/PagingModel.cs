using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class PagingModel
    {
        [JsonProperty("count")]
        public long Count { get; set; }
        [JsonProperty("start")]
        public long Start { get; set; }
        [JsonProperty("total")]
        public long Total { get; set; }
    }
}
