using EpicKit.WebAPI.Game.Models.Achievements;
using EpicKit.WebAPI.Game.Models.Auth;
using EpicKit.WebAPI.Game.Models.Catalog;
using EpicKit.WebAPI.Game.Models.Stats;
using EpicKit.WebAPI.Game.Models.TitleStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace EpicKit.WebAPI.Game
{
    public enum GameFeatures : uint
    {
        [EnumMember(Value = "AccountCreation")]
        AccountCreation,
        [EnumMember(Value = "Achievements")]
        Achievements,
        [EnumMember(Value = "AntiCheat")]
        AntiCheat,
        [EnumMember(Value = "Connect")]
        Connect,
        [EnumMember(Value = "Ecom")]
        Ecom,
        [EnumMember(Value = "Leaderboards")]
        Leaderboards,
        [EnumMember(Value = "Lobbies")]
        Lobbies,
        [EnumMember(Value = "Matchmaking")]
        Matchmaking,
        [EnumMember(Value = "Metrics")]
        Metrics,
        [EnumMember(Value = "Notifications")]
        Notifications,
        [EnumMember(Value = "PlayerDataStorage")]
        PlayerDataStorage,
        [EnumMember(Value = "PlayerReports")]
        PlayerReports,
        [EnumMember(Value = "ProgressionSnapshot")]
        ProgressionSnapshot,
        [EnumMember(Value = "ReceiptValidation")]
        ReceiptValidation,
        [EnumMember(Value = "Sanctions")]
        Sanctions,
        [EnumMember(Value = "Stats")]
        Stats,
        [EnumMember(Value = "TitleStorage")]
        TitleStorage,
        [EnumMember(Value = "Voice")]
        Voice,
    }

    public class AchievementThreshold
    {
        public string Name { get; set; }
        public long Threshold { get; set; }
    }

    public class AchievementsInfos
    {
        public string AchievementId { get; set; }
        public SortedDictionary<string, string> UnlockedDisplayName { get; init; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> UnlockedDescription { get; init; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> LockedDisplayName { get; init; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> LockedDescription { get; init; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> FlavorText { get; init; } = new SortedDictionary<string, string>();
        public string UnlockedIconUrl { get; set; }
        public string LockedIconUrl { get; set; }
        public bool IsHidden { get; set; }
        public List<AchievementThreshold> StatsThresholds { get; init; } = new List<AchievementThreshold>();
    }



    public class GameConnection : IDisposable
    {
        private class InternalAchievementsInfos
        {
            public List<AchievementItemV2Model> Achievements { get; set; }
            public string Locale { get; set; }
        }

        HttpClient _WebHttpClient;

        public enum ApiVersion
        {
            v1_0_0,
            v1_1_0,
            v1_2_0,
            v1_3_0,
            v1_3_1,
            v1_5_0,
            v1_6_0,
            v1_6_1,
            v1_6_2,
            v1_7_0,
            v1_7_1,
            v1_8_0,
            v1_8_1,
            v1_9_0,
            v1_10_0,
            v1_10_1,
            v1_10_2,
            v1_10_3,
            v1_11_0,
            v1_12_0,
            v1_13_0,
            v1_13_1,
            v1_14_0,
            v1_14_1,
            v1_14_2,
            v1_15_0,
            v1_15_1,
            v1_15_2,
            v1_15_3,
            v1_15_4,
            v1_15_5,
            v1_16_0,
            v1_16_1,
        }

        GameAuthModel _GameAuth;
        GameOAuthModel _GameOAuthDetails;
        GameOAuthLoginModel _GameOAuthLoginDetails;

        string _ApiVersion;
        string _UserAgent;

        public string GameUserId { get; private set; }
        public string GamePassword { get; private set; }
        public string DeploymentId => _GameAuth?.DeploymentId;
        public string _Nonce { get; private set; }

        public string AccountId => _GameOAuthDetails?.AccountId;
        public string ProductUserId => _GameOAuthLoginDetails?.ProductUserId;
        public string Namespace => _GameOAuthLoginDetails?.SandboxId;

        public string GameAccessToken => _GameOAuthLoginDetails.AccessToken;

        bool _LoggedIn;

        public GameConnection()
        {
            _WebHttpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
            });

            _ApiVersion = string.Empty;

            GameUserId = string.Empty;
            GamePassword = string.Empty;
            _Nonce = string.Empty;

            _LoggedIn = false;
        }

        public void Dispose()
        {
            _WebHttpClient.Dispose();
        }

        public string ApiVersionToString(ApiVersion v)
        {
            switch (v)
            {
                case ApiVersion.v1_0_0: return "1.0.0-5464091";
                case ApiVersion.v1_1_0: return "1.1.0-6537116";
                case ApiVersion.v1_2_0: return "1.2.0-9765216";
                case ApiVersion.v1_3_0: return "1.3.0-11034880";
                case ApiVersion.v1_3_1: return "1.3.1-11123224";
                case ApiVersion.v1_5_0: return "1.5.0-12496671";
                case ApiVersion.v1_6_0: return "1.6.0-13289764";
                case ApiVersion.v1_6_1: return "1.6.1-13568552";
                case ApiVersion.v1_6_2: return "1.6.2-13619780";
                case ApiVersion.v1_7_0: return "1.7.0-13812567";
                case ApiVersion.v1_7_1: return "1.7.1-13992660";
                case ApiVersion.v1_8_0: return "1.8.0-14316386";
                case ApiVersion.v1_8_1: return "1.8.1-14507409";
                case ApiVersion.v1_9_0: return "1.9.0-14547226";
                case ApiVersion.v1_10_0: return "1.10.0-14778275";
                case ApiVersion.v1_10_1: return "1.10.1-14934259";
                case ApiVersion.v1_10_2: return "1.10.2-15217776";
                case ApiVersion.v1_10_3: return "1.10.3-15571429";
                case ApiVersion.v1_11_0: return "1.11.0-15929945";
                case ApiVersion.v1_12_0: return "1.12.0-16488214";
                case ApiVersion.v1_13_0: return "1.13.0-16697186";
                case ApiVersion.v1_13_1: return "1.13.1-16972539";
                case ApiVersion.v1_14_0: return "1.14.0-17607641";
                case ApiVersion.v1_14_1: return "1.14.1-18153445";
                case ApiVersion.v1_14_2: return "1.14.2-18950192";
                case ApiVersion.v1_15_0: return "1.15.0-20662730";
                case ApiVersion.v1_15_1: return "1.15.1-20662730";
                case ApiVersion.v1_15_2: return "1.15.2-21689671";
                case ApiVersion.v1_15_3: return "1.15.3-21924193";
                case ApiVersion.v1_15_4: return "1.15.4-23143668";
                case ApiVersion.v1_15_5: return "1.15.5-24099393";
                case ApiVersion.v1_16_0: return "1.16.0-27024038";
                case ApiVersion.v1_16_1: return "1.16.1-27379709";
            }

            return ApiVersionToString(ApiVersion.v1_16_1);
        }

        void _MakeNonce(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            _Nonce = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task _GameAuthAsync(string deployementId)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>( "grant_type", "client_credentials" ),
                new KeyValuePair<string, string>( "deployment_id", deployementId ),
            });

            var json = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/oauth/token"), content, new Dictionary<string, string>
            {
                { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion },
            }));

            if (json.ContainsKey("errorCode"))
                WebApiException.BuildErrorFromJson(json);

            _GameAuth = json.ToObject<GameAuthModel>();
        }

        private async Task _GameOAuthAsync(AuthToken token)
        {
            var content = default(FormUrlEncodedContent);

            switch (token.Type)
            {
                case AuthToken.TokenType.ExchangeCode:
                    content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>( "grant_type"   , "exchange_code" ),
                            new KeyValuePair<string, string>( "scope"        , "openid" ),
                            new KeyValuePair<string, string>( "exchange_code", token.Token ),
                            new KeyValuePair<string, string>( "deployment_id", DeploymentId ),
                        });
                    break;

                case AuthToken.TokenType.RefreshToken:
                    content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>( "grant_type"   , "refresh_token" ),
                            new KeyValuePair<string, string>( "scope"        , "openid" ),
                            new KeyValuePair<string, string>( "refresh_token", token.Token ),
                            new KeyValuePair<string, string>( "deployment_id", DeploymentId ),
                        });
                    break;
            }

            var json = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/epic/oauth/v1/token"), content, new Dictionary<string, string>
            {
                { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion },
            }));

            if (json.ContainsKey("errorCode"))
            {
                try
                {
                    WebApiException.BuildErrorFromJson(json);
                }
                catch (WebApiException e)
                {
                    if (json.ContainsKey("continuation") && e.ErrorCode == WebApiException.OAuthScopeConsentRequired)
                        throw new WebApiException((string)json["continuation"], WebApiException.OAuthScopeConsentRequired);

                    throw;
                }
            }

            _GameOAuthDetails = json.ToObject<GameOAuthModel>();
        }

        private async Task _CreateProductUserIdAsync(string continuationToken)
        {
            var json = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/users"), new StringContent(string.Empty), new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {continuationToken}" },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion },
            }));

            if (json.ContainsKey("errorCode"))
                WebApiException.BuildErrorFromJson(json);
        }

        private async Task _GameLoginWithOAuthTokenAsync()
        {
            _MakeNonce(22);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>( "grant_type", "external_auth" ),
                new KeyValuePair<string, string>( "external_auth_type", "epicgames_access_token" ),
                new KeyValuePair<string, string>( "external_auth_token", _GameOAuthDetails.AccessToken ),
                new KeyValuePair<string, string>( "deployment_id", DeploymentId ),
                new KeyValuePair<string, string>( "nonce", _Nonce ),
            });

            var json = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/oauth/token"), content, new Dictionary<string, string>
            {
                { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion },
            }));

            if (!json.ContainsKey("errorCode"))
            {
                _GameOAuthLoginDetails = json.ToObject<GameOAuthLoginModel>();
            }
            else
            {
                try
                {
                    WebApiException.BuildErrorFromJson(json);
                }
                catch(WebApiException ex)
                {
                    if (ex.ErrorCode != WebApiException.EOSUserNotFound)
                        throw;

                    // If we have the error UserNotFound, then we need to create the product user instead of logging in.
                    await _CreateProductUserIdAsync((string)json["continuation_token"]);
                }
            }
        }

        private async Task _GameLoginAsync(string deployementId, string userId, string password, AuthToken token, ApiVersion apiVersion)
        {
            try
            {
                _ApiVersion = ApiVersionToString(apiVersion);
                _UserAgent = $"EOS-SDK/{_ApiVersion} (Linux/) Unreal/1.0.0";

                GameUserId = userId;
                GamePassword = password;

                await _GameAuthAsync(deployementId);
                await _GameOAuthAsync(token);
                await _GameLoginWithOAuthTokenAsync();

                _LoggedIn = true;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }
        }

        public Task<string> RunContinuationTokenAsync(string continuation_token, string deployement_id, string user_id, string password) =>
            Shared.RunContinuationToken(_WebHttpClient, continuation_token, deployement_id, user_id, password);

        public Task GameLoginWithExchangeCodeAsync(string deployement_id, string user_id, string password, string exchange_code, ApiVersion api_version = ApiVersion.v1_15_3) =>
            _GameLoginAsync(deployement_id, user_id, password, new AuthToken { Token = exchange_code, Type = AuthToken.TokenType.ExchangeCode }, api_version);

        public Task GameLoginWithRefreshTokenAsync(string deployement_id, string user_id, string password, string game_token, ApiVersion api_version = ApiVersion.v1_15_3) =>
            _GameLoginAsync(deployement_id, user_id, password, new AuthToken { Token = game_token, Type = AuthToken.TokenType.RefreshToken }, api_version);

        public bool HasFeature(GameFeatures feature) => _GameAuth.Features.Contains(feature);

        public async Task<List<AchievementsInfos>> GetAchievementsSchemaAsync(IEnumerable<string> requestedLocales = null, int parallelTasks = 5, int version = 2)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            var result = new List<AchievementsInfos>();

            if (!HasFeature(GameFeatures.Achievements))
                return result;

            try
            {
                List<string> locales;

                if (requestedLocales?.Any() != true)
                {
                    locales = new List<string>
                    {
                        "ar",//  Arabic
                        "cs",//  Czech
                        "de",//  German
                        "en",//  English
                        "es-ES", // Spanish - Spain
                        "es-MX", // Spanish - Mexico
                        "fr",//  French
                        "it",
                        "ja",//  Japanese
                        "ko",//  Korean
                        "pl",
                        "pt-BR", // Portugues - Brasil
                        "ru",
                        "th",
                        "tr",
                        "zh",
                        "zh-Hant"
                    };
                }
                else
                {
                    locales = requestedLocales.ToList();
                }

                var languagesTasks = new List<Task>();
                var achievementInfosList = new List<InternalAchievementsInfos>(locales.Count);

                for (int i = 0; i < locales.Count; ++i)
                {
                    while (languagesTasks.Count >= parallelTasks)
                    {
                        languagesTasks.Remove(await Task.WhenAny(languagesTasks));
                    }

                    var currentLocale = locales[i];
                    var task = Task.Run(async () =>
                    {
                        using var webClient = new HttpClient();
                        try
                        {
                            var achievementsModel = JArray.Parse(await Shared.WebRunGet(webClient, new HttpRequestMessage(HttpMethod.Get, $"https://{Shared.EGS_DEV_HOST}/stats/v{version}/{DeploymentId}/definitions/achievements?locale={currentLocale}&iconLinks=true"), new Dictionary<string, string> {
                                { "Authorization", $"Bearer {GameAccessToken}" },
                                { "User-Agent"   , _UserAgent },
                                { "X-EOS-Version", _ApiVersion }
                            })).ToObject<List<AchievementItemV2Model>>();

                            achievementInfosList.Add(new InternalAchievementsInfos
                            {
                                Achievements = achievementsModel,
                                Locale = currentLocale,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Achievement web request {currentLocale} failed: {ex.Message}");
                            throw;
                        }
                    });

                    languagesTasks.Add(task);
                }

                await Task.WhenAll(languagesTasks);

                var hasDefault = false;
                foreach (var achievementsInfos in achievementInfosList)
                {
                    foreach (var ach in achievementsInfos.Achievements)
                    {
                        string unlocked_display_name = ach.Achievement.UnlockedDisplayName.GetLocalizedString(achievementsInfos.Locale, out var default_unlocked_display_name);

                        string unlocked_description = ach.Achievement.UnlockedDescription.GetLocalizedString(achievementsInfos.Locale, out var default_unlocked_description);

                        string locked_display_name = ach.Achievement.LockedDisplayName.GetLocalizedString(achievementsInfos.Locale, out var default_locked_display_name);

                        string locked_description = ach.Achievement.LockedDescription.GetLocalizedString(achievementsInfos.Locale, out var default_locked_description);

                        string flavor_text = ach.Achievement.FlavorText.GetLocalizedString(achievementsInfos.Locale, out var default_flavor_text);

                        var hasLanguage = !default_unlocked_display_name ||
                            !default_unlocked_description ||
                            !default_locked_display_name ||
                            !default_locked_description ||
                            !default_flavor_text;

                        if (!hasDefault)
                        {
                            var unlocked_icon_id = ach.Achievement.UnlockedIconId.Default;
                            var locked_icon_id = ach.Achievement.LockedIconId.Default;

                            var achievements = new AchievementsInfos
                            {
                                AchievementId = ach.Achievement.Name,
                                UnlockedDisplayName = new SortedDictionary<string, string> { { "default", ach.Achievement.UnlockedDisplayName.Default } },
                                UnlockedDescription = new SortedDictionary<string, string> { { "default", ach.Achievement.UnlockedDescription.Default } },
                                LockedDisplayName = new SortedDictionary<string, string> { { "default", ach.Achievement.LockedDisplayName.Default } },
                                LockedDescription = new SortedDictionary<string, string> { { "default", ach.Achievement.LockedDescription.Default } },
                                FlavorText = new SortedDictionary<string, string> { { "default", ach.Achievement.FlavorText.Default } },
                                UnlockedIconUrl = ach.IconLinks.ContainsKey(unlocked_icon_id) ? ach.IconLinks[unlocked_icon_id].ReadLink.ToString() : null,
                                LockedIconUrl = ach.IconLinks.ContainsKey(locked_icon_id) ? ach.IconLinks[locked_icon_id].ReadLink.ToString() : null,
                                IsHidden = ach.Achievement.Hidden,
                                StatsThresholds = ach.Achievement.StatThresholds.Select(st => new AchievementThreshold
                                {
                                    Name = st.Key,
                                    Threshold = st.Value,
                                }).ToList()
                            };
                            achievements.StatsThresholds.Sort((st1, st2) => st1.Name.CompareTo(st2.Name));

                            result.Add(achievements);
                        }

                        var achievement = result.Find(a => a.AchievementId == ach.Achievement.Name);

                        if (!default_unlocked_display_name)
                            achievement.UnlockedDisplayName[achievementsInfos.Locale] = unlocked_display_name;

                        if (!default_unlocked_description)
                            achievement.UnlockedDescription[achievementsInfos.Locale] = unlocked_description;

                        if (!default_locked_display_name)
                            achievement.LockedDisplayName[achievementsInfos.Locale] = locked_display_name;

                        if (!default_locked_description)
                            achievement.LockedDescription[achievementsInfos.Locale] = locked_description;

                        if (!default_flavor_text)
                            achievement.LockedDescription[achievementsInfos.Locale] = flavor_text;
                    }
                    hasDefault = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve achievements: {ex.Message}");
            }

            return result;
        }

        public async Task<List<StatsModel>> GetStatsAsync(int version = 1)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            var result = new List<StatsModel>();

            if (!HasFeature(GameFeatures.Stats))
                return result;

            // $"https://api.epicgames.dev/stats/v{version}/{_DeploymentId}/stats/{ProductUserId}"

            throw new NotImplementedException();
        }

        public async Task<CatalogModel> QueryOffersAsync(int start = 0, int count = 100, int version = 2)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            return JObject.Parse(await Shared.WebRunGet(_WebHttpClient, new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{Shared.EGS_DEV_HOST}/epic/ecom/v{version}/identities/{AccountId}/namespaces/{Namespace}/offers?start={start}&count={count}&locale=en")), new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {GameAccessToken}" },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion }
            })).ToObject<CatalogModel>();
        }

        public async Task QueryEntitlementsAsync(int version = 1)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            if (!HasFeature(GameFeatures.Ecom))
                return;

            // https://api.epicgames.dev/epic/ecom/v{version}/identities/{AccountId}/entitlements?sandboxId={namespace}&start=0&count=100&includeRedeemed=true

            throw new NotImplementedException();
        }

        public async Task<TitleStorageResponseModel> QueryTitleStorageAsync(IEnumerable<string> files, int version = 2)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            if (!HasFeature(GameFeatures.TitleStorage))
                return new TitleStorageResponseModel();

            var content = new StringContent(JsonConvert.SerializeObject(new JObject
            {
                { "files", new JArray(files) }
            }), new UTF8Encoding(false), "application/json");

            return JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/titlestorage/v{version}/match/deployment/{DeploymentId}/titlestorage/?getDuration=300"), content, new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {GameAccessToken}" },
                { "User-Agent"   , _UserAgent },
                { "X-EOS-Version", _ApiVersion }
            })).ToObject<TitleStorageResponseModel>();
        }

        public async Task QueryLeaderboardsAsync(int version = 1)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            if (!HasFeature(GameFeatures.Leaderboards))
                return;

            // https://api.epicgames.dev/leaderboards/v{version}/{_DeploymentId}/definitions/leaderboards

            throw new NotImplementedException();
        }
    }
}