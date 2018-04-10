using System;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class OfferSummaryExtracter : Extracter<OfferSummary>
    {
        private readonly DateRange dateRange;
        private readonly int advertiserId;
        private readonly int offerId;

        public OfferSummaryExtracter(DateRange dateRange, int advertiserId = 0, int offerId = 0)
        {
            this.dateRange = new DateRange(dateRange.FromDate, dateRange.ToDate.AddDays(1));
            //Cake needs dateRange.ToDate to be the day after the last day requested
            this.advertiserId = advertiserId;
            this.offerId = offerId;
        }
        public OfferSummaryExtracter(DateTime date, int advertiserId = 0, int offerId = 0)
        {
            this.dateRange = new DateRange(date, date.AddDays(1));
            this.advertiserId = advertiserId;
            this.offerId = offerId;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting OfferSummaries from {0:d} to {1:d}, AdvId {2}, OffId {3}",
                        dateRange.FromDate, dateRange.ToDate.AddDays(-1), advertiserId, offerId);
            var offerSummaries = CakeMarketingUtility.OfferSummaries(dateRange, advertiserId: advertiserId, offerId: offerId);
            Add(offerSummaries);

            End();
        }
    }
}
