using Newtonsoft.Json;

namespace EpicKit.WebAPI.Store.Models;

internal class OAuthStartDetails
{
    [JsonProperty("access_token")]
    public string Token { get; set; }

    [JsonProperty("expires_in")]
    public uint ExpiresIn { get; set; }

    [JsonProperty("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty("refresh_expires")]
    public uint RefreshExpiresIn { get; set; }

    [JsonProperty("refresh_expires_at")]
    public DateTimeOffset RefreshExpiresAt { get; set; }

    [JsonProperty("account_id")]
    public string AccountId { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("internal_client")]
    public bool InternalClient { get; set; }

    [JsonProperty("client_service")]
    public string ClientService { get; set; }

    [JsonProperty("scope")]
    public List<string> Scope { get; set; }

    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    [JsonProperty("app")]
    public string App { get; set; }

    [JsonProperty("in_app_id")]
    public string InAppId { get; set; }

    [JsonProperty("device_id")]
    public string DeviceId { get; set; }

    [JsonProperty("acr")]
    public string ACR { get; set; }

    [JsonProperty("auth_time")]
    public DateTimeOffset AuthTime { get; set; }
}