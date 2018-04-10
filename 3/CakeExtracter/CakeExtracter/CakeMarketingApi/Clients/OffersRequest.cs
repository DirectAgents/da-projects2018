namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class OffersRequest : ApiRequest
    {
        public OffersRequest()
        {
            sort_field = "offer_id";
            sort_descending = "FALSE";
            offer_name = "";
        }

        public int offer_id { get; set; }
        public string offer_name { get; set; }
        public int advertiser_id { get; set; }
        public int vertical_id { get; set; }
        public int offer_type_id { get; set; }
        public int media_type_id { get; set; }
        public int offer_status_id { get; set; }
        public int tag_id { get; set; }
        public int start_at_row { get; set; }
        public int row_limit { get; set; }
        public string sort_field { get; set; }
        public string sort_descending { get; set; }
    }
}