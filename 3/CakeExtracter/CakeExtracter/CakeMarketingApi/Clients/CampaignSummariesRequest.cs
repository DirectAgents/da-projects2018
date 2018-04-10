using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class CampaignSummariesRequest : ApiRequest
    {
        public CampaignSummariesRequest()
        {
            start_date = DateTime.Today.ToString("MM/dd/yyyy");
            end_date = DateTime.Today.AddDays(1).ToString("MM/dd/yyyy");
            subid_id = "";
        }

        public string start_date { get; set; }
        public string end_date { get; set; }
        public int campaign_id { get; set; }
        public int source_affiliate_id { get; set; }
        public int source_affiliate_tag_id { get; set; }
        public string subid_id { get; set; }
        public int site_offer_id { get; set; }
        public int site_offer_tag_id { get; set; }
        public int source_affiliate_manager_id { get; set; }
        public int brand_advertiser_manager_id { get; set; }
        public int event_id { get; set; }
        public int event_type { get; set; }
    }
}
