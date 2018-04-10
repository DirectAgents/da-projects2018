using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.DoubleClickBidManager.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace DBM
{
    public class DBMUtility : IDataStore
    {
        private const string TOKEN_DELIMITER = "|DBMDBM|";
        public const int NumAlts = 10; // including the default (0)

        private string[] ClientID = new string[NumAlts];
        private string[] ClientSecret = new string[NumAlts];
        private string[] AccessToken = new string[NumAlts];
        private string[] RefreshToken = new string[NumAlts];
        private string[] AltAccountIDs = new string[NumAlts];
        public int WhichAlt { get; set; } // default: 0

        private IEnumerable<string> CreateTokenSets()
        {
            for (int i = 0; i < NumAlts; i++)
                yield return AccessToken[i] + TOKEN_DELIMITER + RefreshToken[i];
        }
        public string[] TokenSets // each string in the array is a combination of Access + Refresh Token
        {
            get { return CreateTokenSets().ToArray(); }
            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    var tokenSet = value[i].Split(new string[] { TOKEN_DELIMITER }, StringSplitOptions.None);
                    AccessToken[i] = tokenSet[0];
                    if (tokenSet.Length > 1)
                        RefreshToken[i] = tokenSet[1];
                }
            }
        }

        // --- Logging ---
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[DBMUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[DBMUtility] " + message);
        }

        // --- Constructors ---
        public DBMUtility()
        {
            Setup();
        }
        public DBMUtility(Action<string> logInfo, Action<string> logError)
            : this()
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }
        private void Setup()
        {
            ClientID[0] = ConfigurationManager.AppSettings["GoogleAPI_ClientId"];
            ClientSecret[0] = ConfigurationManager.AppSettings["GoogleAPI_ClientSecret"];
            for (int i = 1; i < NumAlts; i++)
            {
                AltAccountIDs[i] = PlaceLeadingAndTrailingCommas(ConfigurationManager.AppSettings["GoogleAPI_Alt" + i]);
                ClientID[i] = ConfigurationManager.AppSettings["GoogleAPI_ClientId_Alt" + i];
                ClientSecret[i] = ConfigurationManager.AppSettings["GoogleAPI_ClientSecret_Alt" + i];
            }
        }
        private string PlaceLeadingAndTrailingCommas(string idString)
        {
            if (idString == null || idString.Length == 0)
                return idString;
            return (idString[0] == ',' ? "" : ",") + idString + (idString[idString.Length - 1] == ',' ? "" : ",");
        }

        // for alternative credentials...
        public void SetWhichAlt(string accountId)
        {
            WhichAlt = 0; //default
            for (int i = 1; i < NumAlts; i++)
            {
                if (AltAccountIDs[i] != null && AltAccountIDs[i].Contains(',' + accountId + ','))
                {
                    WhichAlt = i;
                    break;
                }
            }
        }

        // -----

        private ClientSecrets GetClientSecrets()
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = ClientID[WhichAlt],
                ClientSecret = ClientSecret[WhichAlt]
            };
            return clientSecrets;
        }
        private UserCredential GetUserCredential()
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = GetClientSecrets(),
                Scopes = new[] { "https://www.googleapis.com/auth/doubleclickbidmanager" },
                DataStore = this // StoreAsync() will get called whenever the tokens are updated
            });
            var tokens = new TokenResponse
            {
                AccessToken = this.AccessToken[WhichAlt],
                RefreshToken = this.RefreshToken[WhichAlt],
                TokenType = "Bearer",
                IssuedUtc = DateTime.UtcNow,
                ExpiresInSeconds = 3600
            };
            var credential = new UserCredential(flow, "user", tokens);
            return credential;
        }

        // --- test ---

        public void Test()
        {
            var credential = GetUserCredential();
            var service = new DoubleClickBidManagerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DA Client Portal"
            });
            var resource = new QueriesResource(service);
            var request = resource.Listqueries();
            var response = request.Execute();
        }

        // --- Implementation of IDataStore ---

        public Task ClearAsync()
        {
            return Task.Delay(0);
        }
        public Task DeleteAsync<T>(string key)
        {
            return Task.Delay(0);
        }
        public Task<T> GetAsync<T>(string key)
        {
            return null;
        }
        public Task StoreAsync<T>(string key, T value)
        {
            if (value is TokenResponse)
            {
                AccessToken[WhichAlt] = (value as TokenResponse).AccessToken;
                RefreshToken[WhichAlt] = (value as TokenResponse).RefreshToken;
            }
            return Task.Delay(0);
        }
    }
}
