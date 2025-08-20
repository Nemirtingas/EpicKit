using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Achievements;

public class AchievementIconLinksModel
{
    [JsonProperty("readLink")]
    public Uri ReadLink { get; set; }
    [JsonProperty("writeLink")]
    public Uri WriteLink { get; set; }
    [JsonProperty("hash")]
    public string Hash { get; set; }
    [JsonProperty("lastModified")]
    public DateTime? LastModified { get; set; }
    [JsonProperty("Size")]
    public int? Size { get; set; }
    [JsonProperty("fileLocked")]
    public bool? FileLocked { get; set; }
}
