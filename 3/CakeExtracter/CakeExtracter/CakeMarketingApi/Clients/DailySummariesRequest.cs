using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class DailySummariesRequest : ApiRequest
    {
        public DailySummariesRequest()
        {
            start_date = DateTime.Today.ToString("MM/dd/yyyy");
            end_date = DateTime.Today.AddDays(1).ToString("MM/dd/yyyy");
            include_tests = "FALSE";
        }

        public string start_date { get; set; }
        public string end_date { get; set; }
        public int affiliate_id { get; set; }
        public int advertiser_id { get; set; }
        public int offer_id { get; set; }
        public int vertical_id { get; set; }
        public int campaign_id { get; set; }
        public int creative_id { get; set; }
        public int account_manager_id { get; set; }
        public string include_tests { get; set; }
    }
}