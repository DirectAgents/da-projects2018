using System;
using System.Collections.Generic;
using System.Configuration;
using AdRoll.Clients;
using AdRoll.Entities;

namespace AdRoll
{
    public class AdRollUtility
    {
        private readonly string Username = ConfigurationManager.AppSettings["AdRollUsername"];
        private readonly string Password = ConfigurationManager.AppSettings["AdRollPassword"];

        // --- Logging ---
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[AdRollUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[AdRollUtility] " + message);
        }

        // --- Constructors ---
        public AdRollUtility()
        {
        }
        public AdRollUtility(Action<string> logInfo, Action<string> logError)
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }

        // --- Clients ---
        private AdReportClient AdReportClient
        {
            get { return (AdReportClient)GetClient(typeof(AdReportClient)); }
        }
        private CampaignReportClient CampaignReportClient
        {
            get { return (CampaignReportClient)GetClient(typeof(CampaignReportClient)); }
        }
        private AdvertisableReportClient AdvertisableReportClient
        {
            get { return (AdvertisableReportClient)GetClient(typeof(AdvertisableReportClient)); }
        }
        private AttributionReportClient AttributionReportClient
        {
            get { return (AttributionReportClient)GetClient(typeof(AttributionReportClient)); }
        }

        private GetAdvertisablesClient GetAdvertisablesClient
        {
            get { return (GetAdvertisablesClient)GetClient(typeof(GetAdvertisablesClient)); }
        }
        private GetAdClient GetAdClient
        {
            get { return (GetAdClient)GetClient(typeof(GetAdClient)); }
        }

        private Dictionary<string, ApiClient> ClientsDict = new Dictionary<string, ApiClient>();
        private ApiClient GetClient(Type clientType)
        {
            var key = clientType.Name;
            if (!ClientsDict.ContainsKey(key))
            {
                ClientsDict[key] = (ApiClient)Activator.CreateInstance(clientType);
                SetupApiClient(ClientsDict[key]);
            }
            return ClientsDict[key];
        }

        private void SetupApiClient(ApiClient apiClient)
        {
            apiClient.SetCredentials(Username, Password);
            apiClient.SetLogging(m => LogInfo(m), m => LogError(m));
        }

        // --- Methods ---

        // Daily Summaries by Ad
        public List<AdSummary> AdDailySummaries(DateTime date, string advertisableEid)
        {
            //Note: We can only do one day at a time because the API allows either:
            // - breakdown by ad for a specified date or daterange (and advertisable)
            // - breakdown by day for a specified ad or group of ads (within the specified advertisable)
            // (but not both) ... so we'd need to know the Eid for each ad and make a call for each one.
            var request = new AdReportRequest
            {
                start_date = date.ToString("MM-dd-yyyy"),
                end_date = date.ToString("MM-dd-yyyy"),
                advertisables = advertisableEid
            };
            var response = this.AdReportClient.AdSummaries(request);
            if (response == null)
            {
                LogInfo("No AdSummaries found");
                return new List<AdSummary>();
            }
            var adSummaries = response.results;

            //Note: As is, the API returns an adsummary for each of the advertisable's ads, even though many of them have all zeros.
            // If we decide not to include zeros, think about the situation where an adsummary was non-zero then later because all-zeros. It wouldn't get updated.

            //bool includeZeros = true;
            //if (!includeZeros)
            //{
            //    adSummaries = new List<AdSummary>();
            //    foreach (var adSum in response.results)
            //    {
            //        if (!adSum.AllZeros())
            //            adSummaries.Add(adSum);
            //    }
            //}

            // Set the date for each summary, b/c the API doesn't include it for this query
            foreach (var adSum in adSummaries)
            {
                adSum.date = date;
            }
            return adSummaries;
        }

        // Daily Summaries by Campaign (also one day per call)
        public List<CampaignSummary> CampaignDailySummaries(DateTime date, string advertisableEid, string campaignEid = null, string externalCampaignEid = null)
        {
            var request = new CampaignReportRequest
            {
                start_date = date.ToString("MM-dd-yyyy"),
                end_date = date.ToString("MM-dd-yyyy"),
                advertisables = advertisableEid,
                campaigns = campaignEid, // null for all campaigns
                external_campaigns = externalCampaignEid
            };
            var response = this.CampaignReportClient.CampaignSummaries(request);
            if (response == null)
            {
                LogInfo("No CampaignSummaries found");
                return new List<CampaignSummary>();
            }
            var campaignSummaries = response.results;
            foreach (var campSum in campaignSummaries)
            {
                campSum.date = date;
            }
            return campaignSummaries;
        }

        // (Daily)AttributionSummaries for an Advertisable
        public List<AttributionSummary> AttributionSummaries(DateTime startDate, DateTime endDate, string advertisableEid)
        {
            var request = new AttributionReportRequest
            {
                start_date = startDate.ToString("MM-dd-yyyy"),
                end_date = endDate.ToString("MM-dd-yyyy"),
                advertisable_eid = advertisableEid
            };
            var response = this.AttributionReportClient.DailySummaries(request);
            if (response == null || response.results == null)
            {
                LogInfo("No AttributionSummaries found for the Advertisable");
                return new List<AttributionSummary>();
            }
            return response.results.date;
        }

        // DailySummaries for an Advertisable
        public List<AdrollDailySummary> AdvertisableDailySummaries(DateTime startDate, DateTime endDate, string advertisableEid)
        {
            var request = new AdvertisableReportRequest
            {
                start_date = startDate.ToString("MM-dd-yyyy"),
                end_date = endDate.ToString("MM-dd-yyyy"),
                advertisables = advertisableEid,
                data_format = "date"
            };
            var response = this.AdvertisableReportClient.DailySummaries(request);
            if (response == null)
            {
                LogInfo("No DailySummaries found for the Advertisable");
                return new List<AdrollDailySummary>();
            }
            return response.results;
        }

        // Stat Summaries by Advertisable
        public List<AdvertisableSummary> AdvertisableSummaries(DateTime startDate, DateTime endDate, IEnumerable<string> advertisableEids)
        {
            var request = new AdvertisableReportRequest
            {
                start_date = startDate.ToString("MM-dd-yyyy"),
                end_date = endDate.ToString("MM-dd-yyyy"),
                advertisables = String.Join(",", advertisableEids),
                data_format = "entity"
            };
            var response = this.AdvertisableReportClient.Summaries(request);
            if (response == null || response.results == null)
            {
                LogInfo("No AdvertisableSummaries found");
                return new List<AdvertisableSummary>();
            }
            return response.results;
        }

        public List<Advertisable> GetAdvertisables()
        {
            var response = this.GetAdvertisablesClient.Get();
            if (response == null)
            {
                LogInfo("No Advertisables found");
                return new List<Advertisable>();
            }
            return response.results;
        }

        public Ad GetAd(string eid)
        {
            var request = new GetAdRequest
            {
                ad = eid
            };
            var response = this.GetAdClient.Get(request);
            if (response == null)
            {
                LogInfo("Ad not found: " + eid);
                return null;
            }
            return response.results;
        }
    }
}
