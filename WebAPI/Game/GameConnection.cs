using EpicKit.WebAPI.Game.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;

namespace EpicKit.WebAPI.Game
{
    [Flags]
    public enum GameFeatures : uint
    {
        None              = 0x00000000,
        Achievements      = 0x00000001,
        AntiCheat         = 0x00000002,
        Connect           = 0x00000004,
        Ecom              = 0x00000008,
        Leaderboards      = 0x00000010,
        Lobbies           = 0x00000020,
        Matchmaking       = 0x00000040,
        Metrics           = 0x00000080,
        PlayerDataStorage = 0x00000100,
        Stats             = 0x00000200,
        TitleStorage      = 0x00000400,
        Voice             = 0x00000800,
    }

    public class AchievementThreshold
    {
        public string Name { get; set; }
        public long Threshold { get; set; }
    }

    public class AchievementsInfos
    {
        public string AchievementId { get; set; }
        public Dictionary<string, string> UnlockedDisplayName { get; init; } = new Dictionary<string, string>();
        public Dictionary<string, string> UnlockedDescription { get; init; } = new Dictionary<string, string>();
        public Dictionary<string, string> LockedDisplayName { get; init; } = new Dictionary<string, string>();
        public Dictionary<string, string> LockedDescription { get; init; } = new Dictionary<string, string>();
        public Dictionary<string, string> FlavorText { get; init; } = new Dictionary<string, string>();
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
        }

        JObject _Json1;
        JObject _Json2;
        JObject _Json3;

        string _ApiVersion;
        string _UserAgent;

        public string GameUserId { get; private set; }
        public string GamePassword { get; private set; }
        public string DeploymentId { get; private set; }
        string _Nonce;

        public string AccountId { get; private set; }
        public string ProductUserId { get; private set; }
        public string Namespace { get; private set; }

        public string GameAccessToken { get; private set; }

        public GameFeatures GameFeatures { get; private set; }

        bool _LoggedIn;

        public GameConnection()
        {
            _WebHttpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
            });
            _Json1 = new JObject();
            _Json2 = new JObject();
            _Json3 = new JObject();

            _ApiVersion = string.Empty;

            GameUserId = string.Empty;
            GamePassword = string.Empty;
            DeploymentId = string.Empty;
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
            }

            return "1.15.3-21924193";
        }

        void _MakeNonce(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            _Nonce = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task _GameLogin(string deployement_id, string user_id, string password, AuthToken token, ApiVersion api_version)
        {
            try
            {
                _ApiVersion = ApiVersionToString(api_version);
                _UserAgent = $"EOS-SDK/{_ApiVersion} (Linux/) Unreal/1.0.0";

                GameUserId = user_id;
                GamePassword = password;
                DeploymentId = deployement_id;

                Uri auth_uri = new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/oauth/token");
                Uri epic_uri = new Uri($"https://{Shared.EGS_DEV_HOST}/epic/oauth/v1/token");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>( "grant_type", "client_credentials" ),
                    new KeyValuePair<string, string>( "deployment_id", DeploymentId ),
                });

                _Json1 = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, auth_uri, content, new Dictionary<string, string>
                {
                    { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                    { "User-Agent"   , _UserAgent },
                    { "X-EOS-Version", _ApiVersion },
                }));

                if (_Json1.ContainsKey("errorCode"))
                    WebApiException.BuildErrorFromJson(_Json1);

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

                _Json2 = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, epic_uri, content, new Dictionary<string, string>
                {
                    { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                    { "User-Agent"   , _UserAgent },
                    { "X-EOS-Version", _ApiVersion },
                }));

                if (_Json2.ContainsKey("errorCode"))
                {
                    try
                    {
                        WebApiException.BuildErrorFromJson(_Json2);
                    }
                    catch (WebApiException e)
                    {
                        if (_Json2.ContainsKey("continuation") && e.ErrorCode == WebApiException.OAuthScopeConsentRequired)
                        {
                            var ex = new WebApiException((string)_Json2["continuation"], WebApiException.OAuthScopeConsentRequired);
                            throw ex;
                        }

                        throw;
                    }
                }

                _MakeNonce(22);
                content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>( "grant_type", "external_auth" ),
                    new KeyValuePair<string, string>( "external_auth_type", "epicgames_access_token" ),
                    new KeyValuePair<string, string>( "external_auth_token", (string)_Json2["access_token"] ),
                    new KeyValuePair<string, string>( "deployment_id", DeploymentId ),
                    new KeyValuePair<string, string>( "nonce", _Nonce ),
                });

                _Json3 = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/oauth/token"), content, new Dictionary<string, string>
                {
                    { "Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{GameUserId}:{GamePassword}"))) },
                    { "User-Agent"   , _UserAgent },
                    { "X-EOS-Version", _ApiVersion },
                }));

                if (_Json3.ContainsKey("errorCode"))
                {
                    try
                    {
                        WebApiException.BuildErrorFromJson(_Json3);
                    }
                    catch(WebApiException ex)
                    {
                        if (ex.ErrorCode != WebApiException.EOSUserNotFound)
                            throw;

                        // if we have the error UserNotFound, then we need to create the product user instead of logging in.
                        _Json3 = JObject.Parse(await Shared.WebRunPost(_WebHttpClient, new Uri($"https://{Shared.EGS_DEV_HOST}/auth/v1/users"), new StringContent(string.Empty), new Dictionary<string, string>
                        {
                            { "Authorization", (string)_Json3["continuation_token"] },
                            { "User-Agent"   , _UserAgent },
                            { "X-EOS-Version", _ApiVersion },
                        }));

                        if (_Json3.ContainsKey("errorCode"))
                            WebApiException.BuildErrorFromJson(_Json3);
                    }
                }

                AccountId = (string)_Json2["account_id"];
                ProductUserId = (string)_Json3["product_user_id"];

                GameAccessToken = (string)_Json3["access_token"];
                Namespace = (string)_Json3["sandbox_id"];

                if (_Json1.ContainsKey("features"))
                {
                    GameFeatures = GameFeatures.None;
                    foreach (var feature in new GameFeatures[] { GameFeatures.Achievements, GameFeatures.AntiCheat, GameFeatures.Connect, GameFeatures.Ecom })
                    {
                        foreach (var jtoken in (JArray)_Json1["features"])
                        {
                            if ((string)jtoken == feature.ToString())
                                GameFeatures |= feature;
                        }
                    }
                }

                _LoggedIn = true;
            }
            catch (Exception e)
            {
                WebApiException.BuildExceptionFromWebException(e);
            }
        }

        public Task<string> RunContinuationToken(string continuation_token, string deployement_id, string user_id, string password) =>
            Shared.RunContinuationToken(_WebHttpClient, continuation_token, deployement_id, user_id, password);

        public Task GameLoginWithExchangeCodeAsync(string deployement_id, string user_id, string password, string exchange_code, ApiVersion api_version = ApiVersion.v1_15_3) =>
            _GameLogin(deployement_id, user_id, password, new AuthToken { Token = exchange_code, Type = AuthToken.TokenType.ExchangeCode }, api_version);

        public Task GameLoginWithRefreshTokenAsync(string deployement_id, string user_id, string password, string game_token, ApiVersion api_version = ApiVersion.v1_15_3) =>
            _GameLogin(deployement_id, user_id, password, new AuthToken { Token = game_token, Type = AuthToken.TokenType.RefreshToken }, api_version);

        public async Task<List<AchievementsInfos>> GetAchievementsSchemaAsync(IEnumerable<string> requestedLocales = null, int parallelTasks = 5, int version = 2)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            var result = new List<AchievementsInfos>();

            if (!GameFeatures.HasFlag(GameFeatures.Achievements))
                return result;

            try
            {
                JArray json;
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
                            bool x;
                            var unlocked_icon_id = ach.Achievement.UnlockedIconId.Default;
                            var locked_icon_id = ach.Achievement.LockedIconId.Default;

                            result.Add(new AchievementsInfos
                            {
                                AchievementId       = ach.Achievement.Name,
                                UnlockedDisplayName = new Dictionary<string, string> { { "default", ach.Achievement.UnlockedDisplayName.Default } },
                                UnlockedDescription = new Dictionary<string, string> { { "default", ach.Achievement.UnlockedDescription.Default } },
                                LockedDisplayName   = new Dictionary<string, string> { { "default", ach.Achievement.LockedDisplayName.Default } },
                                LockedDescription   = new Dictionary<string, string> { { "default", ach.Achievement.LockedDescription.Default } },
                                FlavorText          = new Dictionary<string, string> { { "default", ach.Achievement.FlavorText.Default } },
                                UnlockedIconUrl     = ach.IconLinks.ContainsKey(unlocked_icon_id) ? ach.IconLinks[unlocked_icon_id].ReadLink.ToString() : null,
                                LockedIconUrl       = ach.IconLinks.ContainsKey(locked_icon_id) ? ach.IconLinks[locked_icon_id].ReadLink.ToString() : null,
                                IsHidden            = ach.Achievement.Hidden,
                                StatsThresholds     = ach.Achievement.StatThresholds.Select(st => new AchievementThreshold
                                {
                                    Name = st.Key,
                                    Threshold = st.Value,
                                }).ToList()
                            });
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

            if (!GameFeatures.HasFlag(GameFeatures.Stats))
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

            if (!GameFeatures.HasFlag(GameFeatures.Ecom))
                return;

            // https://api.epicgames.dev/epic/ecom/v{version}/identities/{AccountId}/entitlements?sandboxId={namespace}&start=0&count=100&includeRedeemed=true

            throw new NotImplementedException();
        }

        public async Task<TitleStorageResponseModel> QueryTitleStorageAsync(IEnumerable<string> files, int version = 2)
        {
            if (!_LoggedIn)
                throw new WebApiException("User is not logged in.", WebApiException.NotLoggedIn);

            if (!GameFeatures.HasFlag(GameFeatures.TitleStorage))
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

            if (!GameFeatures.HasFlag(GameFeatures.Leaderboards))
                return;

            // https://api.epicgames.dev/leaderboards/v{version}/{_DeploymentId}/definitions/leaderboards

            throw new NotImplementedException();
        }
    }
}