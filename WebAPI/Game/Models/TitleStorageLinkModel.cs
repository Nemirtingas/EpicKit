﻿using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models
{
    public class TitleStorageLinkModel
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}
