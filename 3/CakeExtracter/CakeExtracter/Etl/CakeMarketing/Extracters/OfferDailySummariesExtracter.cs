using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class OfferDailySummariesExtracter : Extracter<OfferDailySummary>
    {
        private readonly DateRange dateRange;
        private readonly IEnumerable<int> advertiserIds;
        private readonly int? offerId;
        private readonly bool skipDeletionsIfAll;
        private readonly int itemsPerBatch;

        // Note: You can pass in null for advertiserIds - for all advertisers or if offerId is specified

        public OfferDailySummariesExtracter(DateRange dateRange, IEnumerable<int> advertiserIds, int? offerId, bool skipDeletionsIfAll, int itemsPerBatch = 20)
        {
            this.dateRange = dateRange;
            this.advertiserIds = advertiserIds ?? new List<int> { 0 };
            this.offerId = offerId;
            this.skipDeletionsIfAll = skipDeletionsIfAll;
            this.itemsPerBatch = itemsPerBatch;
        }

        public OfferDailySummariesExtracter(DateRange dateRange, int advertiserId, int? offerId, bool skipDeletionsIfAll, int itemsPerBatch = 20)
        {
            this.dateRange = dateRange;
            this.advertiserIds = new List<int> { advertiserId };
            this.offerId = offerId;
            this.skipDeletionsIfAll = skipDeletionsIfAll;
            this.itemsPerBatch = itemsPerBatch;
        }

        protected override void Extract()
        {
            foreach (var advIdBatch in advertiserIds.InBatches(itemsPerBatch))
            {
                Parallel.ForEach(advIdBatch, advertiserId =>
                {
                    List<int> offerIds;
                    if (this.offerId.HasValue)
                        offerIds = new List<int> { offerId.Value };
                    else
                        offerIds = CakeMarketingUtility.OfferIds(advertiserId);

                    if (offerIds.Count == 0)
                        Logger.Info("Adv {0} - no offers", advertiserId);
                    else
                        Logger.Info("Adv {0} - Extracting DailySummaries for {1} offerIds between {2} and {3}: {4}",
                                    advertiserId,
                                    offerIds.Count,
                                    dateRange.FromDate.ToShortDateString(),
                                    dateRange.ToDate.ToShortDateString(),
                                    string.Join(", ", offerIds));

                    foreach (var offIdBatch in offerIds.InBatches(itemsPerBatch))
                    {
                        Parallel.ForEach(offIdBatch, offId =>
                        {
                            var dailySummaries = CakeMarketingUtility.DailySummaries(dateRange, advertiserId, offId, 0, 0);

                            Logger.Info("Extracted {0} DailySummaries for offerId={1}", dailySummaries.Count, offId);

                            Add(dailySummaries.Select(c => new OfferDailySummary(offId, c)));

                            if (dailySummaries.Count > 0 || !skipDeletionsIfAll)
                            {
                                // Check for dates with no data
                                var datesExtracted = dailySummaries.Select(ds => ds.Date);
                                for (var iDate = dateRange.FromDate; iDate < dateRange.ToDate; iDate = iDate.AddDays(1))
                                {
                                    if (!datesExtracted.Contains(iDate))
                                        Add(new OfferDailySummary(offId, iDate));
                                }
                            }
                        });
                    }

                });
            }

            End();
        }
    }
}