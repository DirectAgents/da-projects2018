using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using Amazon.Entities;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace Amazon
{
    public class AmazonUtility
    {
        // From Config File
        private readonly string _amazonClientId = ConfigurationManager.AppSettings["AmazonClientId"];
        private readonly string _amazonClientSecret = ConfigurationManager.AppSettings["AmazonClientSecret"];
        //private readonly string _amazonApiUsername = ConfigurationManager.AppSettings["AmazonAPIUsername"];
        //private readonly string _amazonApiPassword = ConfigurationManager.AppSettings["AmazonAPIPassword"];
        private readonly string _amazonApiEndpointUrl = ConfigurationManager.AppSettings["AmazonAPIEndpointUrl"];
        private readonly string _amazonAuthorizeUrl = ConfigurationManager.AppSettings["AmazonAuthorizeUrl"];
        private readonly string _amazonTokenUrl = ConfigurationManager.AppSettings["AmazonTokenUrl"];
        private readonly string _amazonClientUrl = ConfigurationManager.AppSettings["AmazonClientUrl"];
        //private readonly string _amazonAccessCode = ConfigurationManager.AppSettings["AmazonAccessCode"];
        private readonly string _amazonRefreshToken = ConfigurationManager.AppSettings["AmazonRefreshToken"];

        private const string TOKEN_DELIMITER = "|AMZNAMZN|";
        public const int NumAlts = 10; // including the default (0)
        public const string CAMPAIGNTYPE_SPONSOREDPRODUCTS = "sponsoredProducts";
        public const string CAMPAIGNTYPE_HSA = "headlineSearch";

        //private long CustomerID { get; set; }
        //private string DeveloperToken { get; set; }
        //private string UserName { get; set; }
        //private string Password { get; set; }
        //private string ClientId { get; set; }
        //private string ClientSecret { get; set; }

        private string[] AuthCode = new string[NumAlts];
        private string[] AccessToken = new string[NumAlts];
        private string[] RefreshToken = new string[NumAlts];
        private string[] AltAccountIDs = new string[NumAlts];
        public int WhichAlt { get; set; } // default: 0
        
        private string ApiEndpointUrl { get; set; }
        private string AuthorizeUrl { get; set; }
        private string TokenUrl { get; set; }
        private string ClientUrl { get; set; }
        //private string ProfileId { get; set; }
        //public static string AccessToken { get; set; }
        //public static string RefreshToken { get; set; }
        //public static string ApplicationAccessCode { get; set; }

        //private AmazonAuth AmazonAuth = null;

        #region Logging
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[AmazonUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[AmazonUtility] " + message);
        } 
        #endregion

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

        private void ResetCredentials()
        {
            //UserName = _amazonApiUsername;
            //Password = _amazonApiPassword;
            //ClientId = _amazonApiClientId;
            //ClientSecret = _amazonClientSecret;
            ApiEndpointUrl = _amazonApiEndpointUrl;
            AuthorizeUrl = _amazonAuthorizeUrl;
            TokenUrl = _amazonTokenUrl;
            ClientUrl = _amazonClientUrl;
            //ApplicationAccessCode = _amazonAccessCode;
            //RefreshToken = _amazonRefreshToken;
        }

        #region Constructors
        public AmazonUtility()
        {
            ResetCredentials();
            //AmazonAuth = new AmazonAuth(_amazonApiClientId, _amazonClientSecret, _amazonAccessCode);
            Setup();
        }
        public AmazonUtility(Action<string> logInfo, Action<string> logError)
            : this()
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }
        private void Setup()
        {
            AuthCode[0] = ConfigurationManager.AppSettings["AmazonAuthCode"];
            for (int i = 1; i < NumAlts; i++)
            {
                AltAccountIDs[i] = PlaceLeadingAndTrailingCommas(ConfigurationManager.AppSettings["Amazon_Alt" + i]);
                AuthCode[i] = ConfigurationManager.AppSettings["AmazonAuthCode_Alt" + i];
            }
        }
        private string PlaceLeadingAndTrailingCommas(string idString)
        {
            if (idString == null || idString.Length == 0)
                return idString;
            return (idString[0] == ',' ? "" : ",") + idString + (idString[idString.Length - 1] == ',' ? "" : ",");
        }
        #endregion

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

        // Use the refreshToken if we have one, otherwise use the auth code
        public void GetAccessToken()
        {
            var restClient = new RestClient
            {
                BaseUrl = new Uri(_amazonTokenUrl),
                Authenticator = new HttpBasicAuthenticator(_amazonClientId, _amazonClientSecret)
            };
            restClient.AddHandler("application/x-www-form-urlencoded", new JsonDeserializer());

            var request = new RestRequest();
            request.AddParameter("redirect_uri", "https://portal.directagents.com");
            if (String.IsNullOrWhiteSpace(RefreshToken[WhichAlt]))
            {
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("code", AuthCode[WhichAlt]);
            }
            else
            {
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("refresh_token", RefreshToken[WhichAlt]);
            }
            var response = restClient.ExecuteAsPost<GetTokenResponse>(request, "POST");

            if (response.Data == null || response.Data.access_token == null)
                LogError("Failed to get access token");

            if (response.Data != null && response.Data.refresh_token == null)
                LogError("Failed to get refresh token");

            if (response.Data != null)
            {
                AccessToken[WhichAlt] = response.Data.access_token;
                RefreshToken[WhichAlt] = response.Data.refresh_token; // update this in case it changed
            }
        }

        private IRestResponse<T> ProcessRequest<T>(RestRequest restRequest, bool postNotGet = false)
            where T : new()
        {
            var restClient = new RestClient
            {
                BaseUrl = new Uri(_amazonApiEndpointUrl)
            };
            //restClient.AddHandler("application/json", new JsonDeserializer());

            if (String.IsNullOrEmpty(AccessToken[WhichAlt]))
                GetAccessToken();

            restRequest.AddHeader("Authorization", "bearer " + AccessToken[WhichAlt]);
            //restRequest.AddHeader("Content-Type", "application/json");
            restRequest.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            bool done = false;
            int tries = 0;
            IRestResponse<T> response = null;
            while (!done)
            {
                if (postNotGet)
                    response = restClient.ExecuteAsPost<T>(restRequest, "POST");
                else
                    response = restClient.ExecuteAsGet<T>(restRequest, "GET");

                //var jsonResponse = JsonConvert.DeserializeObject(response.Content);
                tries++;

                if (response.StatusCode == HttpStatusCode.Unauthorized && tries < 2)
                {
                    // Get a new access token and use that.
                    GetAccessToken();

                    var param = restRequest.Parameters.Find(p => p.Type == ParameterType.HttpHeader && p.Name == "Authorization");
                    param.Value = "bearer " + AccessToken[WhichAlt];
                }
                else if (response.StatusDescription != null && response.StatusDescription.Contains("IN_PROGRESS") && tries < 5)
                { 
                    LogInfo("API calls quota exceeded. Waiting 5 seconds.");
                    Thread.Sleep(5000);
                }
                else
                    done = true; //TODO: distinguish between success and failure of ProcessRequest
            }
            if (!String.IsNullOrWhiteSpace(response.ErrorMessage))
                LogError(response.ErrorMessage);

            return response;
        }

        public List<Profile> GetProfiles()
        {
            try
            {
                List<Profile> profiles = new List<Profile>();
                var client = new RestClient(_amazonApiEndpointUrl);
                client.AddHandler("application/json", new JsonDeserializer());
                var request = new RestRequest("v1/profiles", Method.GET);
                var restResponse = ProcessRequest<List<Profile>>(request, postNotGet: false);
                return restResponse.Data;
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return null;
        }

        //Get campaigns by profile id
        // for sponsored product campaigns only - as of v.20180312
        public List<AmazonCampaign> GetCampaigns(string profileId)
        {
            try
            {
                var client = new RestClient(_amazonApiEndpointUrl); //"https://advertising-api.amazon.com"
                client.AddHandler("application/json", new JsonDeserializer());
                var request = new RestRequest("v1/campaigns", Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Amazon-Advertising-API-Scope", profileId);
                var restResponse = ProcessRequest<List<AmazonCampaign>>(request, postNotGet: false);
                return restResponse.Data;
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return null;
        }

        public List<AmazonKeyword> GetKeywords(string profileId)
        {
            try
            {
                var client = new RestClient(_amazonApiEndpointUrl); //"https://advertising-api.amazon.com"
                client.AddHandler("application/json", new JsonDeserializer());
                var request = new RestRequest("v1/keywords", Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Amazon-Advertising-API-Scope", profileId);
                var restResponse = ProcessRequest<List<AmazonKeyword>>(request, postNotGet: false);
                return restResponse.Data;
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return null;
        }

        public List<AmazonProductAd> GetProductAds(string profileId)
        {
            try
            {
                var client = new RestClient(_amazonApiEndpointUrl); //"https://advertising-api.amazon.com"
                client.AddHandler("application/json", new JsonDeserializer());
                var request = new RestRequest("v1/productAds", Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Amazon-Advertising-API-Scope", profileId);
                var restResponse = ProcessRequest<List<AmazonProductAd>>(request, postNotGet: false);
                return restResponse.Data;
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return null;
        }

        public AmazonApiReportParams CreateAmazonApiReportParams(string campaignType, DateTime date, bool includeCampaignName = false)
        {
            var reportParams = new AmazonApiReportParams
            {
                campaignType = campaignType,
                //segment = segment,
                reportDate = date.ToString("yyyyMMdd"),
                metrics = "cost,impressions,clicks,attributedConversions14d,attributedSales14d" + (includeCampaignName ? ",campaignName" : "")
            };
            return reportParams;
        }
        public ReportRequestResponse SubmitReport(AmazonApiReportParams reportParams, string recordType, string profileId)
        {
            var request = new RestRequest("v1/" + recordType + "/report");
            request.AddHeader("Amazon-Advertising-API-Scope", profileId);
            request.AddJsonBody(reportParams);

            var restResponse = ProcessRequest<ReportRequestResponse>(request, postNotGet: true);
            if (restResponse != null && restResponse.Content != null)
            {

                //ReportResponse reportResponse = restResponse.Data;
                return restResponse.Data;
            }
            return null;
        }

        // returns json string (or null)
        public string WaitForReportAndDownload(string reportId, string profileId)
        {
            int triesLeft = 60; // 5 minutes
            while (triesLeft > 0)
            {
                var downloadInfo = RequestReport(reportId, profileId);
                if (downloadInfo != null && !String.IsNullOrWhiteSpace(downloadInfo.location))
                {
                    var json = GetJsonStringFromDownloadFile(downloadInfo.location, profileId);
                    return json;
                }
                triesLeft--;
            }
            return null;
        }

        public ReportResponseDownloadInfo RequestReport(string reportId, string profileId)
        {
            try
            {
                var request = new RestRequest("v1/reports/" + reportId);
                request.AddHeader("Amazon-Advertising-API-Scope", profileId);

                var restResponse = ProcessRequest<ReportResponseDownloadInfo>(request, postNotGet: false);
                if (restResponse.Data.status == "SUCCESS")
                    return restResponse.Data;
                else if (restResponse.Content.Contains("IN_PROGRESS"))
                {
                    LogInfo("Waiting 5 seconds for report to finish generating.");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return null;
        }

        public string GetJsonStringFromDownloadFile(string url, string profileId)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("Authorization", "bearer " + AccessToken[WhichAlt]);
                request.Headers.Add("Amazon-Advertising-API-Scope", profileId);
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var streamReader = new StreamReader(responseStream);
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(exePath, "download.gzip");
                using (Stream s = File.Create(filePath))
                {
                    responseStream.CopyTo(s);
                }
                FileInfo fileToDecompress = new FileInfo(filePath);
                Decompress(fileToDecompress);
                string jsonFile = Path.Combine(exePath, "download.json");
                using (StreamReader r = new StreamReader(jsonFile))
                {
                    string json = r.ReadToEnd();
                    return json;
                }
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return string.Empty;           
        }

        public void Decompress(FileInfo fileToDecompress)
        {
            try
            {
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = File.Create(newFileName + ".json"))
                    {
                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                            //Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                        }
                    }
                }
            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
        }
    }
}
