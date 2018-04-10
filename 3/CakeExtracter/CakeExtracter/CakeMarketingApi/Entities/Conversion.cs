using System;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Conversion
    {
        public string ConversionId { get; set; }
        public DateTime ConversionDate { get; set; }
        public DateTime ClickDate { get; set; }
        public int ClickId { get; set; }
        public Affiliate1 Affiliate { get; set; }
        public Advertiser1 Advertiser { get; set; }
        public Offer1 Offer { get; set; }
        public Received Received { get; set; }
        public string TransactionId { get; set; }
        //VisitorId, RequestSessionId...
        //Campaign, Creative...
    }
}