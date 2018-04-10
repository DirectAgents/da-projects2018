namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class CampaignsRequest : ApiRequest
    {
        public CampaignsRequest()
        {
            sort_field = "campaign_id";
            sort_descending = "FALSE";
            media_type_id = "0";
        }

        public int campaign_id { get; set; }
        public int offer_id { get; set; }
        public int affiliate_id { get; set; }
        public int account_status_id { get; set; }
        public string media_type_id { get; set; }
        public int start_at_row { get; set; }
        public int row_limit { get; set; }
        public string sort_field { get; set; }
        public string sort_descending { get; set; }
    }
}
