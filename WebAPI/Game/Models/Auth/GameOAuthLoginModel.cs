using Newtonsoft.Json;

namespace EpicKit.WebAPI.Game.Models.Auth;

public class GameOAuthLoginModel
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("token_type")]
    public string TokenType { get; set; }
    [JsonProperty("expires_at")]
    public DateTime expires_at { get; set; }
    [JsonProperty("nonce")]
    public string Nonce { get; set; }
    [JsonProperty("features")]
    public List<GameFeatures> Features { get; set; }
    [JsonProperty("organization_id")]
    public string OrganizationId { get; set; }
    [JsonProperty("product_id")]
    public string ProductId { get; set; }
    [JsonProperty("sandbox_id")]
    public string SandboxId { get; set; }
    [JsonProperty("deployment_id")]
    public string DeploymentId { get; set; }
    [JsonProperty("organization_user_id")]
    public string OrganizationUserId { get; set; }
    [JsonProperty("product_user_id")]
    public string ProductUserId { get; set; }
    [JsonProperty("product_user_id_created")]
    public bool ProductUserIdCreated { get; set; }
    [JsonProperty("id_token")]
    public string IdToken { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}
