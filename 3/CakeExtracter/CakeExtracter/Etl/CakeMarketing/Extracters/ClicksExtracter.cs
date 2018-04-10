using System;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class ClicksExtracter : Extracter<Click>
    {
        private readonly DateRange dateRange;
        private readonly int advertiserId;

        public ClicksExtracter(DateRange dateRange, int advertiserId)
        {
            this.dateRange = dateRange;
            this.advertiserId = advertiserId;
        }

        protected override void Extract()
        {
            Logger.Info("Getting offerIds for advertiserId={0}", advertiserId);

            var offerIds = CakeMarketingUtility.OfferIds(advertiserId);

            Logger.Info("Extracting Clicks for {0} offerIds between {2} and {3}: {1}",
                offerIds.Count,
                string.Join(", ", offerIds),
                dateRange.FromDate.ToShortDateString(),
                dateRange.ToDate.ToShortDateString());

            foreach (var offerId in offerIds)
            {
                var clicks = RetryUtility.Retry(3, 10000, new[] { typeof(Exception) }, () =>
                        CakeMarketingUtility.EnumerateClicks(dateRange, advertiserId, offerId));

                foreach (var clickBatch in clicks.InBatches(1000))
                {
                    Logger.Info("Extracted {0} Clicks for offerId={1}", clickBatch.Count, offerId);

                    Add(clickBatch);
                }
            }

            End();
        }
    }
}
