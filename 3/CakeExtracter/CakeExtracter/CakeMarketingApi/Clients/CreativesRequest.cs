namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class CreativesRequest : ApiRequest
    {
        public CreativesRequest()
        {
            sort_field = "creative_id";
            sort_descending = "FALSE";
            creative_name = "";
            creative_type_id = "0";
            creative_status_id = "0";
        }

        public int creative_id { get; set; }
        public string creative_name { get; set; }
        public int offer_id { get; set; }
        public string creative_type_id { get; set; }
        public string creative_status_id { get; set; }
        public int start_at_row { get; set; }
        public int row_limit { get; set; }
        public string sort_field { get; set; }
        public string sort_descending { get; set; }
    }
}
