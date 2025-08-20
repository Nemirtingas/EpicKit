using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Catalog;

public class CatalogItemKeyImageModel
{
    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "url")]
    public string Url { get; set; }

    [JsonProperty(PropertyName = "md5")]
    public string Md5 { get; set; }

    [JsonProperty(PropertyName = "width")]
    public int Width { get; set; }

    [JsonProperty(PropertyName = "height")]
    public int Height { get; set; }

    [JsonProperty(PropertyName = "size")]
    public int Size { get; set; }

    [JsonProperty(PropertyName = "uploadedDate")]
    public DateTime UploadedDate { get; set; }
}
