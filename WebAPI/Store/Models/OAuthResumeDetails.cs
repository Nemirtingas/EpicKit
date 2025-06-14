using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

internal class OAuthResumeDetails
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("expires_in")]
    public uint ExpiresIn { get; set; }

    [JsonProperty("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("account_id")]
    public string AccountId { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("internal_client")]
    public string InternalClient { get; set; }

    [JsonProperty("client_service")]
    public string ClientService { get; set; }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("app")]
    public string App { get; set; }

    [JsonProperty("in_app_id")]
    public string InAppId { get; set; }

    [JsonProperty("device_id")]
    public string DeviceId { get; set; }

    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}