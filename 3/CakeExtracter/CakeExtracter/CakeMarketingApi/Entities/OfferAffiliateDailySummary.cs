using System;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class OfferAffiliateDailySummary
    {
        public OfferAffiliateDailySummary()
        {
        }

        public OfferAffiliateDailySummary(int offerId, int affiliateId, DailySummary dailySummary)
        {
            OfferId = offerId;
            AffiliateId = affiliateId;
            DailySummary = dailySummary;
        }

        public OfferAffiliateDailySummary(int offerId, int affiliateId, DateTime deleteDate)
        {
            OfferId = offerId;
            AffiliateId = affiliateId;
            DeleteDate = deleteDate;
        }

        public int OfferId { get; set; }
        public int AffiliateId { get; set; }
        public DailySummary DailySummary { get; set; }

        // if not null, means delete the dailysummary for this date
        public DateTime? DeleteDate { get; set; }
    }
}
