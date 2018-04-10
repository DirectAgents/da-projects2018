using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class ArrayOfTraffic
    {
        public List<Traffic> Traffics { get; set; }
    }

    public class Traffic
    {
        public int AdvertiserId { get; set; }

        public string AdvertiserName { get; set; }

        public int OfferId { get; set; }

        public string OfferName { get; set; }

        public int AffiliateId { get; set; }

        public string AffiliateName { get; set; }

        public int CampaignId { get; set; }

        public string VerticalName { get; set; }

        public string CategoryName { get; set; }

        public int Impressions { get; set; }

        public int Clicks { get; set; }

        public int Conversions { get; set; }

        public decimal Revenue { get; set; }

        public decimal Cost { get; set; }

        public List<Rate> AdvertiserRates { get; set; }

        public List<Rate> AffiliateRates { get; set; }
    }

    public class Rate
    {
        public decimal Price { get; set; }

        public int Conversions { get; set;}
    }
}
