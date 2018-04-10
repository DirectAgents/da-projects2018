using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class CampaignSummaryExtracter : Extracter<CampaignSummary>
    {
        private readonly DateRange dateRange;
        private readonly int[] offerIds;
        private readonly bool groupByOffAff;
        private readonly bool getDailyStats;

        //TODO: pass in an array for offerIds - or use "params"

        public CampaignSummaryExtracter(DateRange dateRange, int offerId = 0, bool groupByOffAff = false, bool getDailyStats = false)
        {
            this.dateRange = dateRange;
            this.offerIds = new[] { offerId };
            this.groupByOffAff = groupByOffAff;
            this.getDailyStats = getDailyStats;
        }
        //public CampaignSummaryExtracter(DateTime date, int offerId = 0, bool groupByOffAff = false)
        //{
        //    this.dateRange = new DateRange(date, date.AddDays(1));
        //    this.offerIds = new[] { offerId };
        //    this.groupByOffAff = groupByOffAff;
        //}

        protected override void Extract()
        {
            Logger.Info("Extracting CampaignSummaries from {0:d} to {1:d}, OffIds {2}",
                        dateRange.FromDate, dateRange.ToDate, string.Join(",", offerIds));
            if (getDailyStats)
            {
                foreach (var date in dateRange.Dates)
                {
                    LoopThroughOffers(new DateRange(date, date.AddDays(1)));
                }
            }
            else
            {
                LoopThroughOffers(new DateRange(dateRange.FromDate, dateRange.ToDate.AddDays(1)));
            }
            End();
            //Note: Cake needs dateRange.ToDate to be the day after the last day of stats needed
        }
        private void LoopThroughOffers(DateRange dateRangeForStats)
        {
            foreach (var offerId in offerIds)
            {
                var campaignSummaries = CakeMarketingUtility.CampaignSummaries(dateRangeForStats, offerId: offerId);
                if (this.groupByOffAff)
                    ExtractWithGrouping(campaignSummaries, dateRangeForStats.FromDate);
                else
                    ExtractWithoutGrouping(campaignSummaries, dateRangeForStats.FromDate);
            }
            //Note: If not getting daily stats, the CampaignSummaries will have Date set to the first date in the daterange
        }

        private void ExtractWithoutGrouping(IEnumerable<CampaignSummary> campaignSummaries, DateTime date)
        {
            foreach (var campSum in campaignSummaries)
            {
                if (campSum.SiteOffer.SiteOfferId < 0)
                    continue; // skip OfferIds -2 and -1

                //TODO: filter those with zero conversions, rev, cost... etc
                //TODO: allow for CampSums marked for deletion - w/ CampSumWrap?

                campSum.Date = date;
                Add(campSum);
            }
        }
        private void ExtractWithGrouping(IEnumerable<CampaignSummary> campaignSummaries, DateTime date)
        {
            var csOfferGroups = campaignSummaries.GroupBy(cs => cs.SiteOffer.SiteOfferId);

            foreach (var group in csOfferGroups)
            {
                if (group.Key < 0)
                    continue; // skip OfferIds -2 and -1

                var affIds = group.Select(cs => cs.SourceAffiliate.SourceAffiliateId).Distinct();
                foreach (var affId in affIds)
                {
                    var cSums = group.Where(cs => cs.SourceAffiliate.SourceAffiliateId == affId);
                    if (cSums.Count() == 1)
                    {
                        cSums.First().Date = date;
                        Add(cSums);
                    }
                    else
                    { // There's more than one for this off/aff combo.  Sum up the stats.
                        var totals = new CampaignSummary
                        {
                            Views = cSums.Sum(cs => cs.Views),
                            Clicks = cSums.Sum(cs => cs.Clicks),
                            MacroEventConversions = cSums.Sum(cs => cs.MacroEventConversions),
                            Paid = cSums.Sum(cs => cs.Paid),
                            Sellable = cSums.Sum(cs => cs.Sellable),
                            Cost = cSums.Sum(cs => cs.Cost),
                            Revenue = cSums.Sum(cs => cs.Revenue)
                        };
                        var first = cSums.First();
                        first.CopyStatsFrom(totals);
                        first.Date = date;
                        Add(first);
                        // (Assume they all have the same AccountManager,Advertiser,AdvertiserManager,etc though only the stats are loaded.)
                    }
                }
            }
        }
    }
}
