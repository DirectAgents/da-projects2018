using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Apple;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;
using RestSharp;
using RestSharp.Deserializers;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestAppleCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestAppleCommand()
        {
            IsCommand("testApple");
        }

        public override int Execute(string[] remainingArguments)
        {
            //FillStats();
            return 0;
        }

        public void FillStats()
        {
            int searchAccountId = 184; // BritBox

            var start = new DateTime(2017, 8, 25);
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(start, yesterday);

            using (var db = new ClientPortalContext())
            {
                var campIds = db.SearchCampaigns.Where(x => x.SearchAccountId == searchAccountId).Select(x => x.SearchCampaignId).ToArray();

                var sdsQuery = db.SearchDailySummaries.Where(x => campIds.Contains(x.SearchCampaignId));

                foreach (var date in dateRange.Dates)
                {
                    var sdsForDate = sdsQuery.Where(x => x.Date == date).Select(x => x.SearchCampaignId).Distinct().ToArray();
                    var missingCampIds = campIds.Where(x => !sdsForDate.Contains(x)).ToArray();
                    foreach (var campId in missingCampIds)
                    {
                        var sds = new SearchDailySummary
                        {
                            SearchCampaignId = campId,
                            Date = date,
                            Network = ".",
                            Device = ".",
                            CurrencyId = 1
                        };
                        db.SearchDailySummaries.Add(sds);
                    }
                    db.SaveChanges();
                    Logger.Info("{0} {1} existed, {2} added", date.ToShortDateString(), sdsForDate.Length, missingCampIds.Length);
                }
            }
        }

        public void Test1()
        {
            var appleAdsUtility = new AppleAdsUtility();
            string orgId = "124790"; //80760-DA, 124790-Crackle
            var startDate = new DateTime(2017, 4, 11);
            var endDate = new DateTime(2017, 4, 12);
            var stats = appleAdsUtility.GetCampaignDailyStats(startDate, endDate, orgId);
        }

        public void Test()
        {
            var restClient = new RestClient("https://api.searchads.apple.com/api/v1/campaigns");
            restClient.AddHandler("application/json", new JsonDeserializer());

            var certificate = new X509Certificate2();
            certificate.Import(@"G:\work\sp\apple\Certificates\AppleCertificateDA.p12", "appleda1", X509KeyStorageFlags.DefaultKeySet);
            restClient.ClientCertificates = new X509CertificateCollection() { certificate };

            var request = new RestRequest();
            request.AddHeader("Authorization", "orgId=124790"); //80760-DA, 124790-Crackle

            var response = restClient.Execute<AppleResponse>(request);
        }

    }
    public class AppleResponse
    {
        public object data { get; set; }
        public Pagination pagination { get; set; }
        public object error { get; set; }
    }
    public class Pagination
    {
        public int totalResults { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
    }
}
