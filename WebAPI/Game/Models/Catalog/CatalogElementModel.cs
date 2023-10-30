using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Catalog
{
    public class CatalogElementModel
    {
        [JsonProperty("availableForPurchase")]
        public bool AvailableForPurchase { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("effectiveDate")]
        public DateTime EffectiveDate { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("items")]
        public List<CatalogItemModel> Items { get; set; }
        [JsonProperty("keyImages")]
        public List<CatalogItemKeyImageModel> KeyImages { get; set; }
        [JsonProperty("namespace")]
        public string Namespace { get; set; }
        [JsonProperty("offerType")]
        public string OfferType { get; set; }
        [JsonProperty("priceInfo")]
        public CatalogItemPriceInfoModel PriceInfo { get; set; }
        [JsonProperty("purchaseLimit")]
        public int PurchaseLimit { get; set; }
        [JsonProperty("purchasedCount")]
        public int PurchasedCount { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
