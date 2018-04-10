namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class AdvertisersRequest : ApiRequest
    {
        public AdvertisersRequest()
        {
            advertiser_name = string.Empty;
            tag_id = "0";
            start_at_row = 1;
            sort_field = "advertiser_id";
            sort_descending = "FALSE";
        }

        //advertiser_id / INT = Advertiser ID ["0" = ALL Advertisers] {See export.asmx > Advertisers}
        public int advertiser_id { get; set; }

        //advertiser_name / STRING = Freeform Search Term
        public string advertiser_name { get; set; }

        //account_manager_id / INT = Advertiser Manager ID ["0" = ALL Advertiser Managers]
        public int account_manager_id { get; set; }

        //tag_id / STRING = Tag ID ["0" = ALL Tags] {See get.asmx > AffiliateTags}
        public string tag_id { get; set; }

        //start_at_row / INT = Starting Row Number [Usually "1", unless doing incremental API Calls]
        public int start_at_row { get; set; }

        //row_limit / INT = Maximum Rows Returned ["0" = ALL Rows, "100000" = Maximum]
        public int row_limit { get; set; }

        //sort_field / STRING = Sort Field ["advertiser_id", "advertiser_name", "date_created"]
        public string sort_field { get; set; }

        //sort_descending / BOOL = Sort Descending? ["TRUE", "FALSE"]
        public string sort_descending { get; set; }
    }
}
