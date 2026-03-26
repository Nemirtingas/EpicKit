using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.TitleStorage;
public class TitleStorageQueryFileListRequest
{
    [JsonProperty("tags")]
    public IEnumerable<string> Tags { get; set; }
}
