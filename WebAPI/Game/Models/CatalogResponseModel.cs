using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class CatalogModel
    {
        [JsonProperty("elements")]
        public List<CatalogElementModel> Elements { get; set; }

        [JsonProperty("paging")]
        public PagingModel Paging { get; set; }
    }
}
