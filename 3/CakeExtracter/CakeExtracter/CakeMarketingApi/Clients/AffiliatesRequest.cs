
namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class AffiliatesRequest : ApiRequest
    {
        public AffiliatesRequest()
        {
            affiliate_name = string.Empty;
            sort_field = "affiliate_id";
            sort_descending = "FALSE";
            tag_id = "0";
        }

        public int affiliate_id { get; set; }
        public string affiliate_name { get; set; }
        public int account_manager_id { get; set; }
        public int start_at_row { get; set; }
        public int row_limit { get; set; }
        public string sort_field { get; set; }
        public string sort_descending { get; set; }
        public string tag_id { get; set; }
    }

    // See https://getcake.freshdesk.com/support/solutions/articles/5000546174-export-affiliates-api-version-5
}
