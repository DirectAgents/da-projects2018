using System;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class ConversionsRequest : ClicksRequest
    {
        public ConversionsRequest()
        {
            sort_field = "conversion_id";
            sort_descending = "FALSE";
        }

        //sort_field / STRING = Sort Field ["conversion_id", "visitor_id", "request_session_id", "click_id", "conversion_date", "transaction_id", "last_updated"]
        public string sort_field { get; set; }

        //sort_descending / BOOL = Sort Descending? ["TRUE", "FALSE"]
        public string sort_descending { get; set; }
    }
}