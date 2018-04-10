using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class DailySummariesExtracter : Extracter<OfferAffiliateDailySummary>
    {
        private readonly DateRange dateRange;
        private readonly int advertiserId;
        private readonly int? offerId;
        private readonly bool checkCakeTraffic;
        private readonly bool includeDeletions;

        public DailySummariesExtracter(DateRange dateRange, int advertiserId, int? offerId, bool checkCakeTraffic, bool includeDeletions)
        {
            this.dateRange = dateRange;
            this.advertiserId = advertiserId;
            this.offerId = offerId;
            this.checkCakeTraffic = checkCakeTraffic;
            this.includeDeletions = includeDeletions;
        }

        protected override void Extract()
        {
            List<int> offerIds;
            if (this.offerId.HasValue)
            {
                offerIds = new List<int> { offerId.Value };
            }
            else if (checkCakeTraffic)
            {
                Logger.Info("AdvId {0} - getting OfferSummaries", advertiserId);

                var offerSummaries = CakeMarketingUtility.OfferSummaries(dateRange, advertiserId);
                offerIds = offerSummaries.Select(os => os.Offer.OfferId)
                            .Where(id => id > -1).ToList();
            }
            else
            {
                if (advertiserId == 0)
                {
                    offerIds = CakeMarketingUtility.OfferIds(advertiserId);
                }
                else
                {
                    using (var db = new ClientPortalContext())
                    {
                        offerIds = db.Offers.Where(o => o.AdvertiserId == advertiserId)
                                            .Select(o => o.OfferId).ToList();
                    }
                }
            }

            if (offerIds.Count == 0)
                Logger.Info("AdvId {0} - no offers", advertiserId);
            else
                Logger.Info("AdvId {0} - Extracting DailySummaries for {1} offerIds between {2} and {3}: {4}",
                                advertiserId,
                                offerIds.Count,
                                dateRange.FromDate.ToShortDateString(),
                                dateRange.ToDate.ToShortDateString(),
                                string.Join(", ", offerIds));

            foreach (var offIdBatch in offerIds.InBatches(20))
            {
                Parallel.ForEach(offIdBatch, offerId =>
                {
                    List<int> affiliateIds = new List<int>();

                    if (checkCakeTraffic) // get the campaigns/affiliateIds from Cake (based on traffic)
                    {
                        var campaignSummaries = CakeMarketingUtility.CampaignSummaries(dateRange, offerId: offerId);
                        affiliateIds = campaignSummaries.Select(cs => cs.SourceAffiliate.SourceAffiliateId).Distinct()
                                        .Where(id => id > -1).ToList();
                    }
                    else // get the campaigns/affiliateIds from the database
                    {
                        using (var db = new ClientPortalContext())
                        {
                            var campaigns = db.Campaigns.Where(c => c.OfferId == offerId);
                            affiliateIds = campaigns.Select(c => c.AffiliateId).Distinct().ToList();
                        }
                    }
                    Logger.Info("Offer {0} has {1} affiliates. Extracting DailySummaries for each offer/affiliate combo...", offerId, affiliateIds.Count);

                    if (includeDeletions)
                    {
                        // Delete dailySummaries for any other affiliateId (for this dateRange)
                        using (var db = new ClientPortalContext())
                        {
                            var dailySummariesToRemove = db.DailySummaries
                                .Where(ds => ds.OfferId == offerId &&
                                                ds.Date >= dateRange.FromDate &&
                                                ds.Date <= dateRange.ToDate &&
                                                !affiliateIds.Contains(ds.AffiliateId));
                            if (dailySummariesToRemove.Any())
                            {
                                foreach (var dailySummary in dailySummariesToRemove)
                                {
                                    db.DailySummaries.Remove(dailySummary);
                                }
                                db.SaveChanges();
                            }
                        }
                    }

                    foreach (var affIdBatch in affiliateIds.InBatches(100))
                    {
                        Parallel.ForEach(affIdBatch, affId =>
                        {
                            var dailySummaries = CakeMarketingUtility.DailySummaries(dateRange, 0, offerId, 0, affId);

                            //Logger.Info("Extracted {0} DailySummaries for offerId={1}, affId={2}", dailySummaries.Count, offerId, affId);

                            Add(dailySummaries.Select(c => new OfferAffiliateDailySummary(offerId, affId, c)));

                            if (includeDeletions)
                            {
                                // Check for dates with no data
                                var datesExtracted = dailySummaries.Select(ds => ds.Date);
                                for (var iDate = dateRange.FromDate; iDate < dateRange.ToDate; iDate = iDate.AddDays(1))
                                {
                                    if (!datesExtracted.Contains(iDate))
                                        Add(new OfferAffiliateDailySummary(offerId, affId, iDate));
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
