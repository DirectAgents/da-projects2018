using System;
using System.Linq;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;
using System.Collections.Generic;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class ConversionsExtracter : Extracter<Conversion>
    {
        private readonly DateRange dateRange;
        private readonly int advertiserId;

        public ConversionsExtracter(DateRange dateRange, int advertiserId)
        {
            this.dateRange = dateRange;
            this.advertiserId = advertiserId;
        }

        protected override void Extract()
        {
            Logger.Info("Getting offerIds for advertiserId={0}", advertiserId);

            var offerIds = CakeMarketingUtility.OfferIds(advertiserId);

            Logger.Info("Extracting Conversions for {0} offerIds between {2} and {3}: {1}",
                           offerIds.Count,
                           string.Join(", ", offerIds),
                           dateRange.FromDate.ToShortDateString(),
                           dateRange.ToDate.ToShortDateString());

            foreach (var offerId in offerIds)
            {
                List<Conversion> conversions;
                try
                {
                    conversions = RetryUtility.Retry(3, 10000, new[] { typeof(Exception) }, () =>
                            CakeMarketingUtility.Conversions(dateRange, advertiserId, offerId));
                }
                catch (Exception ex)
                {
                    Logger.Info("Exception while extracting Conversions for offerid={0} between {1} and {2}",
                                offerId,
                                dateRange.FromDate.ToShortDateString(),
                                dateRange.ToDate.ToShortDateString());
                    throw ex;
                }

                foreach (var batch in conversions.InBatches(1000))
                {
                    Logger.Info("Extracted {0} Conversions for offerId={1}", batch.Count(), offerId);

                    Add(batch);
                }
            }

            End();
        }
    }
}
