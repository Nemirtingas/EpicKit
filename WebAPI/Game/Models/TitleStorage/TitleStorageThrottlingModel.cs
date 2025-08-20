using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.TitleStorage;

public class TitleStorageThrottlingModel
{
    [JsonProperty("folderThrottled")]
    public bool FolderThrottled { get; set; }
    [JsonProperty("maxFileSizeBytes")]
    public long MaxFileSizeBytes { get; set; }
    [JsonProperty("maxFolderSizeBytes")]
    public long MaxFolderSizeBytes { get; set; }
    [JsonProperty("maxFileCount")]
    public long MaxFileCount { get; set; }
}
