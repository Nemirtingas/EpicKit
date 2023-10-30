using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.TitleStorage
{
    public class TitleStorageFileModel
    {
        [JsonProperty("fullPath")]
        public string FullPath { get; set; }
        [JsonProperty("filePath")]
        public string FilePath { get; set; }
        [JsonProperty("getLink")]
        public TitleStorageLinkModel GetLink { get; set; }
        [JsonProperty("putLink")]
        public TitleStorageLinkModel PutLink { get; set; }
        [JsonProperty("metadata")]
        public TitleStorageMetadataModel Metadata { get; set; }
        [JsonProperty("fileLocked")]
        public bool FileLocked { get; set; }
    }
}
