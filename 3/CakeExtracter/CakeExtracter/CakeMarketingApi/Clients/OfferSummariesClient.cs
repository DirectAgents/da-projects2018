using CakeExtracter.CakeMarketingApi.Entities;
using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class OfferSummariesClient : ApiClient
    {
        public OfferSummariesClient()
            : base(2, "reports", "OfferSummary")
        {
        }

        public OfferSummaryReportResponse OfferSummaries(OfferSummariesRequest request)
        {
            var result = Execute<OfferSummaryReportResponse>(request);
            return result;
        }
    }

    public class OfferSummariesRequest : ApiRequest
    {
        public OfferSummariesRequest()
        {
            start_date = DateTime.Today.ToString("MM/dd/yyyy");
            end_date = DateTime.Today.AddDays(1).ToString("MM/dd/yyyy");
            revenue_filter = "conversions_and_events";
        }

        public string start_date { get; set; }
        public string end_date { get; set; }
        public int advertiser_id { get; set; }
        public int advertiser_manager_id { get; set; }
        public int offer_id { get; set; }
        public int offer_tag_id { get; set; }
        public int affiliate_tag_id { get; set; }
        public int event_id { get; set; }
        public string revenue_filter { get; set; } // "conversions_and_events" or "conversions" or "events"
    }
}
