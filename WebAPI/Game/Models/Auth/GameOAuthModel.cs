using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Auth;

public class GameOAuthModel
{
    [JsonProperty("scope")]
    public string Scope { get; set; }
    [JsonProperty("token_type")]
    public string TokenType { get; set; }
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonProperty("id_token")]
    public string IdToken { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }
    [JsonProperty("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    [JsonProperty("refresh_expires_at")]
    public DateTime RefreshExpiresAt { get; set; }
    [JsonProperty("account_id")]
    public string AccountId { get; set; }
    [JsonProperty("client_id")]
    public string ClientId { get; set; }
    [JsonProperty("application_id")]
    public string ApplicationId { get; set; }
    [JsonProperty("selected_account_id")]
    public string SelectedAccountId { get; set; }
    [JsonProperty("merged_accounts")]
    public List<string> MergedAccounts { get; set; }
}