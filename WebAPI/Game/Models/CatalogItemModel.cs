using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class CatalogItemModel
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("developer")]
        public string Developer { get; set; }
        [JsonProperty("entitlementName")]
        public string EntitlementName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("itemType")]
        public string ItemType { get; set; }
        [JsonProperty("keyImages")]
        public List<CatalogItemKeyImageModel> KeyImages { get; set; }
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        [JsonProperty("releaseInfo")]
        public List<CatalogItemReleaseInfoModel> ReleaseInfo { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
