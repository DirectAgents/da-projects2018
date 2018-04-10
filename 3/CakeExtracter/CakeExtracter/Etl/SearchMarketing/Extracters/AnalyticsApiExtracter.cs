using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;
//using Google.Apis.Authentication.OAuth2;
//using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Services;
using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class AnalyticsApiExtracter : Extracter<AnalyticsRow>
    {
        private readonly string profileId;
        private readonly DateTime startDate;
        private readonly DateTime endDate;

        public AnalyticsApiExtracter(string profileId, CakeExtracter.Common.DateRange dateRange)
        {
            this.profileId = profileId;
            this.startDate = dateRange.FromDate;
            this.endDate = dateRange.ToDate;
        }

        protected override void Extract()
        {
            var rows = EnumerateAnalyticsRows(profileId); // SherrillTree: 14958389
            Add(rows);
            End();
        }

        private IEnumerable<AnalyticsRow> EnumerateAnalyticsRows(string profileId)
        {
            string serviceEmail = ConfigurationManager.AppSettings["GoogleAPI_ServiceEmail"];
            string certPath = ConfigurationManager.AppSettings["GoogleAPI_Certificate"];
            var certificate = new X509Certificate2(certPath, "notasecret", X509KeyStorageFlags.Exportable);
            //var provider = new AssertionFlowClient(GoogleAuthenticationServer.Description, certificate)
            //{
            //    ServiceAccountId = serviceEmail,
            //    //Scope = AnalyticsService.Scopes.AnalyticsReadonly.GetStringValue()
            //    Scope = AnalyticsService.Scopes.AnalyticsReadonly.ToString().ToLower()
            //};
            //var auth = new OAuth2Authenticator<AssertionFlowClient>(provider, AssertionFlowClient.GetState);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceEmail)
                {
                    Scopes = new[] { AnalyticsService.Scope.AnalyticsReadonly }
                }.FromCertificate(certificate));

            var service = new AnalyticsService(new BaseClientService.Initializer()
            {
                //Authenticator = auth,
                HttpClientInitializer = credential,
                ApplicationName = "DA Client Portal"
            });
            string startDate = this.startDate.ToString("yyyy-MM-dd");
            string endDate = this.endDate.ToString("yyyy-MM-dd");
            string metrics = "ga:transactions,ga:transactionRevenue";
            DataResource.GaResource.GetRequest request = service.Data.Ga.Get("ga:" + profileId, startDate, endDate, metrics);
            request.Dimensions = "ga:date,ga:adwordsCampaignID,ga:campaign,ga:source";
            request.Filters = "ga:source=~^(bing|google)$;ga:medium!=organic;ga:campaign!@test";
            //request.MaxResults = 

            GaData gaData = request.Execute();
            // TODO: pagination

            List<AnalyticsRow> nonAdWordsRows = new List<AnalyticsRow>();

            foreach (var row in gaData.Rows)
            {
                AnalyticsRow aRow = null;
                try
                {
                    aRow = new AnalyticsRow()
                    {
                        Date = DateTime.ParseExact(row[0], "yyyyMMdd", CultureInfo.InvariantCulture),
                        //CampaignId = int.Parse(row[1]), // note: any 'total/other' rows will be skipped because their CampaignId is "(not set)" and won't be int.Parsed
                        CampaignName = row[2],
                        Source = row[3],
                        Transactions = int.Parse(row[4]),
                        Revenue = decimal.Parse(row[5])
                    };
                    int campaignId;
                    if (int.TryParse(row[1], out campaignId))
                        aRow.CampaignId = campaignId;
                }
                catch (Exception) { }

                // note: skip rows where transactions and revenue are both 0
                if (aRow != null && (aRow.Transactions != 0 || aRow.Revenue != 0))
                {
                    if (aRow.CampaignId.HasValue)
                        yield return aRow;
                    else
                        nonAdWordsRows.Add(aRow);
                }
            }

            // Handle any rows without campaignIds
            if (nonAdWordsRows.Count > 0)
            {
                var campaignGroups = nonAdWordsRows.GroupBy(r => new { name = r.CampaignName.ToLower(), date = r.Date});

                foreach (var group in campaignGroups)
                {
                    // use the first row in the group for the CampaignName (non-ToLowered)
                    var campaignName = group.First().CampaignName;

                    var source = "bing"; // if multiple sources, call it bing
                    var sources = group.Select(r => r.Source).Distinct();
                    if (sources.Count() == 1)
                        source = sources.First();

                    var aRow = new AnalyticsRow()
                    {
                        Date = group.Key.date,
                        CampaignName = campaignName,
                        Source = source,
                        Transactions = group.Sum(r => r.Transactions),
                        Revenue = group.Sum(r => r.Revenue)
                    };
                    yield return aRow;
                }
            }
        }

    }

    public class AnalyticsRow
    {
        public DateTime Date { get; set; }
        public int? CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string Source { get; set; }
        public int Transactions { get; set; }
        public decimal Revenue { get; set; }
    }
}
