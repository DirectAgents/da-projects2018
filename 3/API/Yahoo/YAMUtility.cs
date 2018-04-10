using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace Yahoo
{
    public class YAMUtility
    {
        private const int NUMTRIES_REQUESTREPORT = 12; // 120 sec (2 min)
        private const int NUMTRIES_GETREPORTSTATUS = 36; // 360 sec (6 min)
        private const int WAITTIME_SECONDS = 10;

        private const string TOKEN_DELIMITER = "|YAMYAM|";
        public const int NumAlts = 10; // including the default (0)

        // From Config:
        private string AuthBaseUrl { get; set; }
        private string[] ClientID = new string[NumAlts];
        private string[] ClientSecret = new string[NumAlts];
        private string[] ApplicationAccessCode = new string[NumAlts];
        private string YAMBaseUrl { get; set; }

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
                _LogInfo("[YAMUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[YAMUtility] " + message);
        }

        // --- Constructors ---
        public YAMUtility()
        {
            Setup();
        }
        public YAMUtility(Action<string> logInfo, Action<string> logError)
            : this()
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }
        private void Setup()
        {
            ClientID[0] = ConfigurationManager.AppSettings["YahooClientID"];
            ClientSecret[0] = ConfigurationManager.AppSettings["YahooClientSecret"];
            ApplicationAccessCode[0] = ConfigurationManager.AppSettings["YahooApplicationAccessCode"]; // aka Auth Code
            for (int i = 1; i < NumAlts; i++)
            {
                AltAccountIDs[i] = PlaceLeadingAndTrailingCommas(ConfigurationManager.AppSettings["Yahoo_Alt" + i]);
                ClientID[i] = ConfigurationManager.AppSettings["YahooClientID_Alt" + i];
                ClientSecret[i] = ConfigurationManager.AppSettings["YahooClientSecret_Alt" + i];
                ApplicationAccessCode[i] = ConfigurationManager.AppSettings["YahooApplicationAccessCode_Alt" + i]; // aka Auth Code
            }
            AuthBaseUrl = ConfigurationManager.AppSettings["YahooAuthBaseUrl"];
            YAMBaseUrl = ConfigurationManager.AppSettings["YAMBaseUrl"];
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

        // Use the refreshToken if we have one, otherwise use the auth code
        public void GetAccessToken()
        {
            var restClient = new RestClient
            {
                BaseUrl = new Uri(AuthBaseUrl),
                Authenticator = new HttpBasicAuthenticator(ClientID[WhichAlt], ClientSecret[WhichAlt])
            };
            restClient.AddHandler("application/x-www-form-urlencoded", new JsonDeserializer());

            var request = new RestRequest();
            request.AddParameter("redirect_uri", "oob");
            if (String.IsNullOrWhiteSpace(RefreshToken[WhichAlt]))
            {
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("code", ApplicationAccessCode[WhichAlt]);
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
                BaseUrl = new Uri(YAMBaseUrl)
            };
            restClient.AddHandler("application/json", new JsonDeserializer());

            if (String.IsNullOrEmpty(AccessToken[WhichAlt]))
                GetAccessToken();

            restRequest.AddHeader("X-Auth-Method", "OAUTH");
            restRequest.AddHeader("X-Auth-Token", AccessToken[WhichAlt]);

            bool done = false;
            int tries = 0;
            IRestResponse<T> response = null;
            while (!done)
            {
                if (postNotGet)
                    response = restClient.ExecuteAsPost<T>(restRequest, "POST");
                else
                    response = restClient.ExecuteAsGet<T>(restRequest, "GET");

                tries++;

                if (response.StatusCode == HttpStatusCode.Unauthorized && tries < 2)
                { // Get a new access token and use that.
                    GetAccessToken();
                    var param = restRequest.Parameters.Find(p => p.Type == ParameterType.HttpHeader && p.Name == "X-Auth-Token");
                    param.Value = AccessToken[WhichAlt];
                }
                else
                    done = true; //TODO: distinguish between success and failure of ProcessRequest
            }
            if (!String.IsNullOrWhiteSpace(response.ErrorMessage))
                LogError(response.ErrorMessage);

            return response;
        }

        // ---

        // Keeps checking until the report is ready, then returns the location(url) of the CSV
        private string WaitForReportUrl(string customerReportId)
        {
            if (String.IsNullOrWhiteSpace(customerReportId))
            {
                LogError("Missing Report Id");
                return null;
            }
            LogInfo("YAM Report ID: " + customerReportId);

            GetReportResponse getReportResponse = null;
            var waitTime = new TimeSpan(0, 0, WAITTIME_SECONDS);
            for (int i = 0; i < NUMTRIES_GETREPORTSTATUS; i++)
            {
                LogInfo(String.Format("Will check if the report is ready in {0} seconds...", waitTime.Seconds));
                Thread.Sleep(waitTime);

                getReportResponse = GetReportStatus(customerReportId);
                if (getReportResponse != null && getReportResponse.status != null)
                {
                    if (getReportResponse.status.ToUpper() == "SUCCESS" || getReportResponse.status.ToUpper() == "FAILED")
                        break;
                }
                LogInfo("The report is not yet ready for download.");
            }

            if (getReportResponse != null && getReportResponse.status != null && getReportResponse.status.ToUpper() == "SUCCESS")
                return getReportResponse.url;

            LogInfo("Failed to obtain report URL");
            return null;
        }
        private GetReportResponse GetReportStatus(string reportId)
        {
            var request = new RestRequest("extreport/" + reportId);
            var response = ProcessRequest<GetReportResponse>(request);

            if (response == null)
                return null;
            return response.Data;
        }

        // returns the url of the csv, or null if there was a problem
        public string GenerateReport(ReportPayload payload)
        {
            CreateReportResponse createReportResponse = null;
            bool okay = false;
            int retries = NUMTRIES_REQUESTREPORT; // includes the initial attempt
            while (!okay && retries > 0)
            {
                bool firstTry = (retries == NUMTRIES_REQUESTREPORT);
                createReportResponse = RequestReport(payload, logResponse: !firstTry);
                retries--;

                if (createReportResponse != null && createReportResponse.status != null && createReportResponse.status.ToUpper() == "SUBMITTED")
                    okay = true;
                else
                {
                    var message = "Invalid createReportResponse" + (retries > 0 ? ". Will retry" : "");
                    if (createReportResponse != null)
                        message = message + String.Format(". status: [{0}] customerReportId: [{1}]", createReportResponse.status, createReportResponse.customerReportId);
                    LogError(message);
                    if (retries > 0)
                        Thread.Sleep(new TimeSpan(0, 0, WAITTIME_SECONDS));
                }
            }
            if (!okay)
                return null;

            return WaitForReportUrl(createReportResponse.customerReportId);
        }

        public ReportPayload CreateReportRequestPayload(DateTime startDate, DateTime endDate, int? accountId = null, bool byAdvertiser = false, bool byCampaign = false, bool byLine = false, bool byAd = false, bool byCreative = false, bool byPixelParameter = false)
        {
            //This produced an InvalidTimeZoneException so we're just going with the system timezone, relying on it to be Eastern(daylight savings adjusted)
            //var offset = TimeZoneInfo.FindSystemTimeZoneById(@"Eastern Standard Time\Dynamic DST").BaseUtcOffset;
            //var start = new DateTimeOffset(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, offset);
            //var end = new DateTimeOffset(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, offset);

            var accountList = new List<int>();
            if (accountId.HasValue)
                accountList.Add(accountId.Value);

            var dimensionList = new List<int>();
            if (byAdvertiser)
                dimensionList.Add(Dimension.ADVERTISER);
            if (byCampaign)
                dimensionList.Add(Dimension.CAMPAIGN);
            if (byLine)
                dimensionList.Add(Dimension.LINE);
            if (byAd)
                dimensionList.Add(Dimension.AD);
            if (byCreative)
                dimensionList.Add(Dimension.CREATIVE);
            if (byPixelParameter)
                dimensionList.Add(Dimension.PIXEL_PARAMETER);

            List<int> metricList;
            if (!byPixelParameter)
                metricList = new List<int>(new[] { Metric.IMPRESSIONS, Metric.CLICKS, Metric.ADVERTISER_SPENDING, Metric.CLICK_THROUGH_CONVERSIONS, Metric.VIEW_THROUGH_CONVERSIONS, Metric.ROAS_ACTION_VALUE });
            else
                metricList = new List<int>(new[] { Metric.CLICK_THROUGH_CONVERSIONS, Metric.VIEW_THROUGH_CONVERSIONS });
                // used to obtain the *real* conversion values from the pixel parameter

            var reportOption = new ReportOption
            {
                timezone = Timezone.NEW_YORK,
                currency = Currency.USD,
                accountIds = accountList.ToArray(),
                dimensionTypeIds = dimensionList.ToArray(),
                metricTypeIds = metricList.ToArray()
            };

            var adjustedEndDate = endDate.AddDays(1).AddSeconds(-1);
            var payload = new ReportPayload
            {
                reportOption = reportOption,
                intervalTypeId = IntervalTypeId.DAY,
                dateTypeId = DateTypeId.CUSTOM_RANGE,
                startDate = startDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz"),
                endDate = adjustedEndDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz")
            };
            return payload;
        }

        private CreateReportResponse RequestReport(ReportPayload payload, bool logResponse = false)
        {
            var request = new RestRequest("extreport/");
            request.AddJsonBody(payload);

            var response = ProcessRequest<CreateReportResponse>(request, postNotGet: true);

            if (response == null)
                return null;
            if (logResponse)
            {
                //LogInfo("ResponseStatus: " + response.ResponseStatus.ToString());
                LogInfo("StatusCode: " + response.StatusCode.ToString());
            }
            return response.Data;
        }
    }

}
