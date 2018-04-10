using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class EventConversionsRequest : ApiRequest
    {
        public EventConversionsRequest()
        {
            start_date = DateTime.Today.ToString("MM/dd/yyyy");
            end_date = DateTime.Today.AddDays(1).ToString("MM/dd/yyyy");
            event_type = "all";
            source_type = "all";
            payment_percentage_filter = "both";
            disposition_type = "all";
            source_affiliate_billing_status = "all";
            brand_advertiser_billing_status = "all";
            test_filter = "non_tests";
            start_at_row = 1;
            row_limit = 0;
            sort_field = "event_conversion_date";
            sort_descending = "FALSE";
        }

        public string start_date { get; set; }
        public string end_date { get; set; }
        public string event_type { get; set; }
        public int event_id { get; set; }
        public int source_affiliate_id { get; set; }
        public int brand_advertiser_id { get; set; }
        public int channel_id { get; set; }
        public int site_offer_id { get; set; }
        public int site_offer_contract_id { get; set; }
        public int source_affiliate_tag_id { get; set; }
        public int brand_advertiser_tag_id { get; set; }
        public int site_offer_tag_id { get; set; }
        public int campaign_id { get; set; }
        public int creative_id { get; set; }
        public int price_format_id { get; set; }
        public string source_type { get; set; }
        public string payment_percentage_filter { get; set; }
        public string disposition_type { get; set; }
        public int disposition_id { get; set; }
        public string source_affiliate_billing_status { get; set; }
        public string brand_advertiser_billing_status { get; set; }
        public string test_filter { get; set; }
        public int start_at_row { get; set; }
        public int row_limit { get; set; }
        public string sort_field { get; set; }
        public string sort_descending { get; set; }
    }

    // See https://support.getcake.com/support/solutions/articles/13000035430-reports-eventsconversions-api-version-17
}
