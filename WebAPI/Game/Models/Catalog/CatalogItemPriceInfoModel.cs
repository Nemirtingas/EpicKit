using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Catalog
{
    public class CatalogItemPriceInfoModel
    {
        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
        [JsonProperty("decimals")]
        public int Decimals { get; set; }
        [JsonProperty("discountPercentage")]
        public decimal DiscountPercentage { get; set; }
        [JsonProperty("discountPrice")]
        public int DiscountPrice { get; set; }
        [JsonProperty("originalPrice")]
        public int OriginalPrice { get; set; }
    }
}
