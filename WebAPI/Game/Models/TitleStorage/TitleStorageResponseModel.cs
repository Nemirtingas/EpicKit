using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.TitleStorage;

public class TitleStorageResponseModel
{
    [JsonProperty("files")]
    public List<TitleStorageFileModel> Files { get; set; }
    [JsonProperty("throttling")]
    public TitleStorageThrottlingModel Throttling { get; set; }
}
