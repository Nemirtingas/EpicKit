using EpicKit.WebAPI;
using EpicKit.WebAPI.Store.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace EpicKit
{

    public class SessionAccount
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_at")]
        public DateTimeOffset AccessTokenExpiresAt { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("refresh_expires_at")]
        public DateTimeOffset RefreshExpiresAt { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        internal void UpdateOAuth(OAuthResumeDetails resumeDetails)
        {
            AccessToken = resumeDetails.Token;
            AccessTokenExpiresAt = resumeDetails.ExpiresAt;
            AccountId = resumeDetails.AccountId;
            DisplayName = resumeDetails.DisplayName;
        }

        internal void UpdateOAuth(OAuthStartDetails startDetails)
        {
            AccessToken = startDetails.Token;
            AccessTokenExpiresAt = startDetails.ExpiresAt;
            RefreshToken = startDetails.RefreshToken;
            RefreshExpiresAt = startDetails.RefreshExpiresAt;
            AccountId = startDetails.AccountId;
            DisplayName = startDetails.DisplayName;
        }

        public SessionAccount Clone()
        {
            return new SessionAccount
            {
                AccessToken = AccessToken,
                AccessTokenExpiresAt = AccessTokenExpiresAt,
                RefreshToken = RefreshToken,
                RefreshExpiresAt = RefreshExpiresAt,
                AccountId = AccountId,
                DisplayName = DisplayName,
            };
        }
    }

    public class WebApi : IDisposable
    {
        CookieContainer _WebCookies;
        CookieContainer _UnauthWebCookies;

        HttpClient _WebHttpClient;
        HttpClient _UnauthWebHttpClient;

        string _SessionID = string.Empty;
        SessionAccount _OAuthInfos = new();
        bool _LoggedIn = false;

        public WebApi()
        {
            _WebCookies = new CookieContainer();
            _UnauthWebCookies = new CookieContainer();

            _WebHttpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                CookieContainer = _WebCookies,
            });

            _UnauthWebHttpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                CookieContainer = _UnauthWebCookies,
            });

            _UnauthWebHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "UELauncher/11.0.1-14907503+++Portal+Release-Live Windows/10.0.19041.1.256.64bit");
        }

        public void Dispose()
        {
            _WebHttpClient.Dispose();
        }

        T _ParseJson<T>(Stream s)
        {
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            {
                return new JsonSerializer().Deserialize<T>(reader);
            }
        }

        void _ResetOAuth()
        {
            _OAuthInfos = new();
            _LoggedIn = false;
        }

        async Task<string> _GetXSRFToken()
        {
            try
            {
                Uri uri = new Uri($"https://{Shared.EPIC_GAMES_HOST}/id/api/csrf");

                var response = await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "User-Agent", Shared.EGS_OAUTH_UAGENT },
                });

                foreach (Cookie c in _WebCookies.GetCookies(uri))
                {
                    if (c.Name == "XSRF-TOKEN")
                        return c.Value;
                }

                throw new WebApiException("XSRF-TOKEN cookie not found.", WebApiException.NotFound);
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return string.Empty;
        }

        async Task<string> _GetExchangeCode(string xsrf_token)
        {
            try
            {
                Uri uri = new Uri($"https://{Shared.EPIC_GAMES_HOST}/id/api/exchange/generate");

                JObject response = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, uri, new StringContent("", Encoding.UTF8), new Dictionary<string, string>
                {
                    { "X-XSRF-TOKEN", xsrf_token },
                    { "User-Agent", Shared.EGS_OAUTH_UAGENT },
                }));

                if (!string.IsNullOrEmpty((string)response["code"]))
                    return (string)response["code"];

                throw new WebApiException((string)response["message"], WebApiException.ErrorCodeFromString((string)response["errorCode"]));
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return string.Empty;
        }

        async Task<SessionAccount> _ResumeSession(string access_token)
        {
            Uri uri = new Uri($"https://{Shared.EGS_OAUTH_HOST}/account/api/oauth/verify");

            try
            {
                _WebCookies.GetCookies(uri).Clear();

                var json = _ParseJson<JObject>(await Shared.WebRunGetStream(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "User-Agent", Shared.EGS_OAUTH_UAGENT },
                    { "Authorization", access_token },
                }));

                if (json.ContainsKey("errorCode"))
                    WebApiException.BuildErrorFromJson(json);

                var resultDetails = json.ToObject<OAuthResumeDetails>();
                _OAuthInfos.UpdateOAuth(resultDetails);
                _SessionID = resultDetails.SessionId;
                _LoggedIn = true;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return _OAuthInfos?.Clone();
        }

        async Task _StartSession(AuthToken token)
        {
            var postData = default(FormUrlEncodedContent);
            var result = new SessionAccount();

            switch (token.Type)
            {
                case AuthToken.TokenType.ExchangeCode:
                    postData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>( "grant_type"   , "exchange_code" ),
                        new KeyValuePair<string, string>( "exchange_code", token.Token ),
                        new KeyValuePair<string, string>( "token_type"   , "eg1"),
                    });
                    break;

                case AuthToken.TokenType.RefreshToken:
                    postData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>( "grant_type"   , "refresh_token" ),
                        new KeyValuePair<string, string>( "refresh_token", token.Token ),
                        new KeyValuePair<string, string>( "token_type"   , "eg1"),
                    });
                    break;

                case AuthToken.TokenType.AuthorizationCode:
                    postData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>( "grant_type"   , "authorization_code" ),
                        new KeyValuePair<string, string>( "code"         , token.Token ),
                        new KeyValuePair<string, string>( "token_type"   , "eg1"),
                    });
                    break;

                case AuthToken.TokenType.ClientCredentials:
                    postData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>( "grant_type"   , "client_credentials" ),
                        new KeyValuePair<string, string>( "token_type"   , "eg1"),
                    });
                    break;

                default:
                    throw new WebApiException("Invalid token type.", WebApiException.InvalidParam);
            }

            var uri = new Uri($"https://{Shared.EGS_OAUTH_HOST}/account/api/oauth/token");

            _WebCookies.GetCookies(uri).Clear();

            try
            {
                var json = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, uri, postData, new Dictionary<string, string>
                {
                    { "User-Agent", Shared.EGS_OAUTH_UAGENT },
                    { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Shared.EGS_USER}:{Shared.EGS_PASS}"))) }
                }));

                if (json.ContainsKey("errorCode"))
                    WebApiException.BuildErrorFromJson(json);

                var startDetails = json.ToObject<OAuthStartDetails>();
                result.UpdateOAuth(startDetails);
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            _OAuthInfos = result;
        }

        public async Task<SessionAccount> LoginAnonymous()
        {
            _ResetOAuth();

            await _StartSession(new AuthToken { Type = AuthToken.TokenType.ClientCredentials });
            return await _ResumeSession($"bearer {_OAuthInfos.AccessToken}");
        }

        public async Task<SessionAccount> LoginSID(string sid)
        {
            _ResetOAuth();

            Uri uri = new Uri($"https://{Shared.EPIC_GAMES_HOST}/id/api/set-sid?sid={sid}");

            _WebCookies.GetCookies(uri).Clear();

            try
            {
                string response = await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "User-Agent"           , Shared.EGL_UAGENT },
                    { "X-Epic-Event-Action"  , "login" },
                    { "X-Epic-Event-Category", "login" },
                    { "X-Requested-With"     , "XMLHttpRequest" },
                    { "Authorization"        , string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Shared.EGS_USER}:{Shared.EGS_PASS}"))) },
                });

                string exchange_code = await _GetExchangeCode(await _GetXSRFToken());

                await _StartSession(new AuthToken { Token = exchange_code, Type = AuthToken.TokenType.ExchangeCode });
                return await _ResumeSession($"bearer {_OAuthInfos.AccessToken}");
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<SessionAccount> LoginAuthCode(string auth_code)
        {
            _ResetOAuth();

            Uri uri = new Uri($"https://{Shared.EPIC_GAMES_HOST}");

            _WebCookies.GetCookies(uri).Clear();

            try
            {
                await _StartSession(new AuthToken { Token = auth_code, Type = AuthToken.TokenType.AuthorizationCode });
                return await _ResumeSession($"bearer {_OAuthInfos.AccessToken}");
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<SessionAccount> LoginAsync(string accessToken, DateTimeOffset accessTokenExpiresAt, string refreshToken, DateTimeOffset refreshTokenExpiresAt)
        {
            _ResetOAuth();

            try
            {
                if (accessTokenExpiresAt > DateTime.Now && (accessTokenExpiresAt - DateTime.Now) > TimeSpan.FromMinutes(10))
                {
                    var result = await _ResumeSession($"bearer {accessToken}");
                    if  (result != null)
                    {
                        _OAuthInfos.RefreshToken = refreshToken;
                        _OAuthInfos.RefreshExpiresAt = refreshTokenExpiresAt;
                        result.RefreshToken = refreshToken;
                        result.RefreshExpiresAt = refreshTokenExpiresAt;
                    }
                    return result;
                }

                if (refreshTokenExpiresAt < DateTime.Now)
                    return null;

                await _StartSession(new AuthToken { Token = refreshToken, Type = AuthToken.TokenType.RefreshToken });
                return await _ResumeSession($"bearer {_OAuthInfos.AccessToken}");
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public Task Logout()
        {
            if (_LoggedIn)
            {
                try
                {
                    Uri uri = new Uri($"https://{Shared.EGS_OAUTH_HOST}/account/api/oauth/sessions/kill/{_OAuthInfos.AccessToken}");

                    return _WebHttpClient.DeleteAsync(uri);
                }
                catch (Exception e)
                {
                    WebApiException.BuildExceptionFromWebException(e);
                }

                _ResetOAuth();
            }

            return Task.CompletedTask;
        }

        public async Task<string> GetArtifactServiceTicket(string sandboxId, string artifactId, string label = "Live", string platform = "Windows")
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                Uri uri = new Uri($"https://{Shared.EGS_ARTIFACT_HOST}/artifact-service/api/public/v1/dependency/sandbox/{sandboxId}/artifact/{artifactId}/ticket");

                JObject json = new JObject
                {
                    { "label"           , label },
                    { "expiresInSeconds", 300 },
                    { "platform"        , platform },
                };
                StringContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                JObject response = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, uri, content, new Dictionary<string, string>
                {
                    { "User-Agent"  , Shared.EGL_UAGENT },
                }));

                if (response.ContainsKey("errorCode"))
                {
                    WebApiException.BuildErrorFromJson(response);
                }
                else
                {
                    return (string)response["code"];
                }
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return string.Empty;

            // Only works when logged in anonymously.
            // sandbox_id is the same as the namespace, artifact_id is the same as the app name


            //r = self.session.post(f'https://{self._artifact_service_host}/artifact-service/api/public/v1/dependency/'
            //                      f'sandbox/{sandbox_id}/artifact/{artifact_id}/ticket',
            //                      json = dict(label = label, expiresInSeconds = 300, platform = platform),
            //                      params= dict(useSandboxAwareLabel = 'false'),
            //                      timeout = self.request_timeout)
            //r.raise_for_status()
            //return r.json()
        }

        //def get_game_manifest_by_ticket(self, artifact_id: str, signed_ticket: str, label= 'Live', platform= 'Windows') :
        //    # Based on EOS Helper Windows service implementation.
        //    r = self.session.post(f'https://{self._launcher_host}/launcher/api/public/assets/v2/'
        //                          f'by-ticket/app/{artifact_id}',
        //                          json=dict(platform= platform, label= label, signedTicket= signed_ticket),
        //                          timeout=self.request_timeout)
        //    r.raise_for_status()
        //    return r.json()
        public async Task<string> GetAppExchangeCodeAsync()
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                Uri uri = new Uri($"https://{Shared.EGS_OAUTH_HOST}/account/api/oauth/exchange");

                JObject response = JObject.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "User-Agent", Shared.EGL_UAGENT },
                    { "Authorization", $"bearer {_OAuthInfos.AccessToken}" },
                }));

                if (response.ContainsKey("errorCode"))
                    WebApiException.BuildErrorFromJson(response);

                return (string)response["code"];
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        /// <summary>
        /// Get the refresh token that can be used to start a game.
        /// </summary>
        /// <param name="exchangeCode">Exchange code generated by GetAppExchangeCode.</param>
        /// <param name="deployementId">Application DeploymentId.</param>
        /// <param name="userId">Application ClientId.</param>
        /// <param name="password">Application ClientSecret.</param>
        /// <returns></returns>
        public async Task<string> GetAppRefreshTokenFromExchangeCode(string exchangeCode, string deployementId, string userId, string password, AuthorizationScopes[] scopes)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            JObject response;
            try
            {
                Uri uri = new Uri($"https://{Shared.EGS_DEV_HOST}/epic/oauth/v1/token");

                var formContent = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "exchange_code" ),
                    new KeyValuePair<string, string>( "exchange_code", exchangeCode ),
                    new KeyValuePair<string, string>( "deployment_id", deployementId ),
                };

                if (scopes?.Length > 0)
                    formContent.Add(new KeyValuePair<string, string>("scope", scopes.JoinWithValue(" ")));

                HttpContent content = new FormUrlEncodedContent(formContent);

                response = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, uri, content, new Dictionary<string, string>
                {
                    { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{password}"))) },
                }));
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
                return null;
            }

            if (response.ContainsKey("errorCode"))
            {
                try
                {
                    WebApiException.BuildErrorFromJson(response);
                }
                catch (WebApiException e)
                {
                    if (response.ContainsKey("continuation") && e.ErrorCode == WebApiException.OAuthScopeConsentRequired)
                        throw new WebApiOAuthScopeConsentRequiredException(e.Message) { ContinuationToken = (string)response["continuation"] };

                    throw;
                }
            }

            return (string)response["refresh_token"];
        }

        public async Task<string> RunContinuationToken(string continuationToken, string deployementId, string userId, string password)
        {
            return await Shared.RunContinuationToken(_WebHttpClient, continuationToken, deployementId, userId, password);
        }

        private string _GetGameCommandLine(AuthToken token, string appid)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            if (string.IsNullOrWhiteSpace(_OAuthInfos.DisplayName))
                throw new WebApiException("OAuth infos doesn't contain 'display_name'.", WebApiException.NotFound);

            if (string.IsNullOrWhiteSpace(_OAuthInfos.AccountId))
                throw new WebApiException("OAuth infos doesn't contain 'account_id'.", WebApiException.NotFound);

            string auth_type = string.Empty;
            switch (token.Type)
            {
                case AuthToken.TokenType.ExchangeCode:
                    auth_type = "exchangecode";
                    break;

                case AuthToken.TokenType.RefreshToken:
                    auth_type = "refreshtoken";
                    break;
            }

            try
            {
                return string.Format("-AUTH_LOGIN=unused -AUTH_PASSWORD={0} -AUTH_TYPE={1} -epicapp={2} -epicenv=Prod -EpicPortal -epicusername={3} -epicuserid={4} -epiclocal=en", token.Token, auth_type, appid, _OAuthInfos.DisplayName, _OAuthInfos.AccountId);
            }
            catch (Exception e)
            {
                throw new WebApiException(e.Message, WebApiException.InvalidParam);
            }
        }

        public string GetGameExchangeCodeCommandLine(string exchangeCode, string appid) =>
            _GetGameCommandLine(new AuthToken { Token = exchangeCode, Type = AuthToken.TokenType.ExchangeCode }, appid);

        public string GetGameTokenCommandLine(string refreshToken, string appid) =>
            _GetGameCommandLine(new AuthToken { Token = refreshToken, Type = AuthToken.TokenType.RefreshToken }, appid);

        public async Task<List<ApplicationAsset>> GetApplicationsAssets(string platform = "Windows", string label = "Live")
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                System.Collections.Specialized.NameValueCollection getData = new System.Collections.Specialized.NameValueCollection()
                {
                    { "label", label },
                };
                string q = Shared.NameValueCollectionToQueryString(getData);
                Uri uri = new Uri($"https://{Shared.EGS_LAUNCHER_HOST}/launcher/api/public/assets/{platform}?{q}");

                JArray response = JArray.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {_OAuthInfos.AccessToken}" },
                }));

                List<ApplicationAsset> app_assets = new List<ApplicationAsset>();

                foreach (JObject asset in response)
                {
                    app_assets.Add(asset.ToObject<ApplicationAsset>());
                }

                return app_assets;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<JObject> GetGameManifest(string gameNamespace, string catalogId, string appName, string platform = "Windows", string label = "Live")
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                Uri uri = new Uri($"https://{Shared.EGS_LAUNCHER_HOST}/launcher/api/public/assets/v2/platform/{platform}/namespace/{gameNamespace}/catalogItem/{catalogId}/app/{appName}/label/{label}");

                var json = JObject.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {_OAuthInfos.AccessToken}" },
                }));

                if (json.ContainsKey("errorCode"))
                    WebApiException.BuildErrorFromJson(json);

                return json;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<ManifestDownloadInfos> GetManifestDownloadInfos(string gameNamespace, string catalogItemId, string appName, string platform = "Windows", string label = "Live")
        {
            var manifestResult = await GetGameManifest(gameNamespace, catalogItemId, appName, platform, label);

            var result = new ManifestDownloadInfos();

            result.ManifestHash = (string)manifestResult["elements"][0]["hash"];

            foreach (JObject manifest in manifestResult["elements"][0]["manifests"])
            {
                var manifestUrl = (string)manifest["uri"];
                string baseUrl = manifestUrl.Substring(0, manifestUrl.LastIndexOf('/'));
                if (!result.BaseUrls.Contains(baseUrl))
                    result.BaseUrls.Add(baseUrl);

                var queryParams = new List<string>();
                if (manifest.ContainsKey("queryParams"))
                {
                    foreach (JObject param in manifest["queryParams"])
                    {
                        queryParams.Add($"{param["name"]}={param["value"]}");
                    }
                }

                if (queryParams.Count > 0)
                {
                    manifestUrl = $"{manifestUrl}?{string.Join("&", queryParams)}";
                }

                if (manifestUrl.Contains(".akamaized.net/"))
                {
                    result.ManifestUrls.Insert(0, manifestUrl);
                }
                else
                {
                    result.ManifestUrls.Add(manifestUrl);
                }
            }

            if (result.BaseUrls.Count <= 0)
                throw new WebApiException("Couldn't find base urls.", WebApiException.NotFound);

            if (result.ManifestUrls.Count <= 0)
                throw new WebApiException("Couldn't find manifest urls.", WebApiException.NotFound);

            foreach (var manifestUrl in result.ManifestUrls)
            {
                using (var response = await _UnauthWebHttpClient.GetAsync(manifestUrl))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = response.Content.ReadAsStream())
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            result.ManifestData = ms.ToArray();

                            var manifestHash = SHA1.HashData(result.ManifestData).Aggregate(new StringBuilder(), (sb, v) => sb.Append(v.ToString("x2"))).ToString();

                            if (result.ManifestHash != manifestHash)
                                throw new WebApiException("Manifest hash didn't match", WebApiException.InvalidData);

                            break;
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"Failed to download manifest on url: {manifestUrl.Split("?")[0]}");
                    }
                }
            }
            if (result.ManifestData.Length <= 0)
                throw new WebApiException("Couldn't download manifest.", WebApiException.InvalidData);

            return result;
        }


        //def get_delta_manifest(self, base_url, old_build_id, new_build_id):
        //    """Get optimized delta manifest (doesn't seem to exist for most games)"""
        //    if old_build_id == new_build_id:
        //        return None
        //
        //    r = self.egs.unauth_session.get(f'{base_url}/Deltas/{new_build_id}/{old_build_id}.delta')
        //    return r.content if r.status_code == 200 else None

        public async Task<List<EntitlementModel>> GetUserEntitlements(uint start = 0, uint count = 5000)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                System.Collections.Specialized.NameValueCollection getData = new System.Collections.Specialized.NameValueCollection()
                {
                    { "start", start.ToString() },
                    { "count", count.ToString() },
                };
                string q = Shared.NameValueCollectionToQueryString(getData);
                Uri uri = new Uri($"https://{Shared.EGS_ENTITLEMENT_HOST}/entitlement/api/account/{_OAuthInfos.AccountId}/entitlements?{q}");

                JArray response = JArray.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {_OAuthInfos.AccessToken}" },
                }));

                List<EntitlementModel> entitlements = new List<EntitlementModel>();
                foreach (JObject entitlement in response)
                {
                    entitlements.Add(entitlement.ToObject<EntitlementModel>());
                }

                return entitlements;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<StoreApplicationInfos> GetGameInfos(string game_namespace, string catalog_item_id, bool include_dlcs = true)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            try
            {
                System.Collections.Specialized.NameValueCollection getData = new System.Collections.Specialized.NameValueCollection()
                {
                    { "id", catalog_item_id },
                    { "includeDLCDetails", include_dlcs.ToString() },
                    { "includeMainGameDetails", "true" },
                    { "country", "US" },
                    { "locale", "en" }
                };
                string q = Shared.NameValueCollectionToQueryString(getData);

                Uri uri = new Uri($"https://{Shared.EGS_CATALOG_HOST}/catalog/api/shared/namespace/{game_namespace}/bulk/items?{q}");

                JObject response = JObject.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, uri), new Dictionary<string, string>
                {
                    { "Authorization", $"bearer {_OAuthInfos.AccessToken}" },
                }));

                StoreApplicationInfos appInfos = null;
                foreach (KeyValuePair<string, JToken> v in response)
                {
                    appInfos = ((JObject)v.Value).ToObject<StoreApplicationInfos>();
                }

                return appInfos;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }

            return null;
        }

        public async Task<JObject> GetDefaultApiEndpointsAsync(string platformId = "LNX")
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{Shared.EGS_DEV_HOST}/sdk/v1/default?platformId={platformId}");

            var t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));

            return JObject.Parse(t);
        }

        public async Task<ApplicationInfos> GetApplicationInfosAsync(string application_user)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{Shared.EPIC_GAMES_HOST}/id/api/client/{application_user}");

            var t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                WebApiException.BuildErrorFromJson(JObject.Parse(t));

            return JObject.Parse(t).ToObject<ApplicationInfos>();
        }

        public async Task AutoAcceptContinuationAsync(string deployement_id, string user_id, string password, string continuationToken, AuthorizationScopes[] scopes)
        {
            var endpoints = await GetDefaultApiEndpointsAsync();

            var baseUrl = "https://www.epicgames.com";
            var authorizeUrl = (string)endpoints["client"]["AuthClient"]["AuthorizeContinuationEndpoint"];
            var referrer = new Uri($"{baseUrl}{authorizeUrl}");
            var cookieUri = new Uri($"{baseUrl}/id");

            authorizeUrl = authorizeUrl.Replace("`continuation`", continuationToken);
            authorizeUrl = authorizeUrl.Replace("`continuation", continuationToken);

            if (authorizeUrl.Contains("`"))
                throw new NotImplementedException(authorizeUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUrl);

            var t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();

            // Get reputation and XSRF token
            request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/id/api/reputation");
            request.Headers.Referrer = referrer;
            t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();

            var xsrfToken = _UnauthWebCookies.GetCookies(cookieUri).FirstOrDefault(c => c.Name.ToLower() == "xsrf-token")?.Value ?? throw new Exception("xsrf-token not found.");

            // Not required
            //request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/id/api/location");
            //request.Headers.Referrer = referrer;
            //t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();

            // Setup user
            request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/id/api/client/{user_id}");
            request.Headers.Referrer = referrer;
            request.Headers.TryAddWithoutValidation("Cookie", _UnauthWebCookies.GetCookieHeader(cookieUri));
            request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
            t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));

            // Login user
            request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/id/api/authenticate");
            request.Headers.Referrer = referrer;
            request.Headers.TryAddWithoutValidation("Cookie", _UnauthWebCookies.GetCookieHeader(cookieUri));
            request.Headers.TryAddWithoutValidation("X-Epic-Client-ID", user_id);
            request.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);

            t = await (await _UnauthWebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));

            // Update the continuation sequence
            var postJsonContent = JsonConvert.SerializeObject(new JObject
            {
                { "clientId", user_id },
                { "continuationToken", continuationToken }
            });

            var postContent = new StringContent(postJsonContent, Encoding.UTF8);

            postContent.Headers.TryAddWithoutValidation("Referrer", referrer.OriginalString);
            postContent.Headers.TryAddWithoutValidation("Cookie", _UnauthWebCookies.GetCookieHeader(cookieUri));
            postContent.Headers.TryAddWithoutValidation("X-Epic-Client-ID", user_id);
            postContent.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
            postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            t = await (await _UnauthWebHttpClient.PostAsync($"{baseUrl}/id/api/continuation", postContent)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));

            if (scopes == null || scopes.Length <= 0)
            {
                scopes = (await GetApplicationInfosAsync(user_id)).AllowedScopes.ToArray();
            }

            postJsonContent = JsonConvert.SerializeObject(new JObject
            {
                { "scope", JArray.FromObject(scopes) },
                { "continuation", continuationToken }
            });

            postContent = new StringContent(postJsonContent, Encoding.UTF8);

            postContent.Headers.TryAddWithoutValidation("Referrer", referrer.OriginalString);
            postContent.Headers.TryAddWithoutValidation("Cookie", _UnauthWebCookies.GetCookieHeader(cookieUri));
            postContent.Headers.TryAddWithoutValidation("X-Epic-Client-ID", user_id);
            postContent.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);
            postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            t = await (await _UnauthWebHttpClient.PostAsync($"{baseUrl}/id/api/client/{user_id}/authorize", postContent)).Content.ReadAsStringAsync();
            if (t.Contains("errorCode"))
                EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));
        }

        //public async Task<JObject> GetProductApiEndpointsAsync(string productId, string deployementId, string platformId = "LNX")
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, $"https://{Shared.EGS_DEV_HOST}/sdk/v1/product/{productId}?platformId={platformId}&deploymentId={deployementId}");
        //
        //    var t = await (await _WebHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)).Content.ReadAsStringAsync();
        //    if (t.Contains("errorCode"))
        //        EpicKit.WebApiException.BuildErrorFromJson(JObject.Parse(t));
        //
        //    return JObject.Parse(t);
        //}
    }
}
