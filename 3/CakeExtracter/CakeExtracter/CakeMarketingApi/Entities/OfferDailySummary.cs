using System;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class OfferDailySummary
    {
        public OfferDailySummary()
        {
        }

        public OfferDailySummary(int offerId, DailySummary dailySummary)
        {
            OfferId = offerId;
            DailySummary = dailySummary;
        }

        public OfferDailySummary(int offerId, DateTime deleteDate)
        {
            OfferId = offerId;
            DeleteDate = deleteDate;
        }

        public int OfferId { get; set; }
        public DailySummary DailySummary { get; set; }

        // if not null, means delete the dailysummary for this date
        public DateTime? DeleteDate { get; set; }
    }
}