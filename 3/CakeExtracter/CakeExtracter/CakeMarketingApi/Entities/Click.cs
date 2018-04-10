using System;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Click
    {
        public int ClickId { get; set; }
        public int TotalClicks { get; set; }
        public DateTime ClickDate { get; set; }
        public Affiliate1 Affiliate { get; set; }
        public Advertiser1 Advertiser { get; set; }
        public Offer1 Offer { get; set; }
        //OfferContract, CampaignId, Creative...
        public Country Country { get; set; }
        public Region Region { get; set; }
        public Device Device { get; set; } // sometimes not included
        public Browser Browser { get; set; }
    }
}
