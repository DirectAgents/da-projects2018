using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class CPCExtracter : Extracter<CampaignSummary>
    {
        private readonly DateRange dateRange;
        private readonly int offerId;

        public CPCExtracter(DateRange dateRange, int offerId = 0)
        {
            this.dateRange = new DateRange(dateRange.FromDate, dateRange.ToDate.AddDays(1));
            //Cake needs dateRange.ToDate to be the day after the last day requested
            this.offerId = offerId;
        }
        public CPCExtracter(DateTime date, int offerId = 0)
        {
            this.dateRange = new DateRange(date, date.AddDays(1));
            this.offerId = offerId;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting CampaignSummaries from {0:d} to {1:d}, OffId {2}",
                        dateRange.FromDate, dateRange.ToDate.AddDays(-1), offerId);

            var campaignSummaries = CakeMarketingUtility.CampaignSummaries(dateRange, offerId: offerId);
            var csOfferGroups = campaignSummaries.GroupBy(cs => cs.SiteOffer.SiteOfferId);

            foreach (var group in csOfferGroups)
            {
                if (group.Key < 0)
                    continue; // skip OfferIds -2 and -1

                var oIds = group.Select(cs => cs.SiteOffer.SiteOfferId).Distinct();
                foreach (var oId in oIds)
                {
                    Logger.Info("Offer ID: " + oId.ToString());
                }
                var affIds = group.Select(cs => cs.SourceAffiliate.SourceAffiliateId).Distinct();
                foreach (var affId in affIds)
                {
                    var cSums = group.Where(cs => cs.SourceAffiliate.SourceAffiliateId == affId);
                    Logger.Info("Affiliate Id: " + affId.ToString());
                    if (cSums.Count() == 1)
                    {
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
                        Add(first);
                        // (Assume they all have the same AccountManager,Advertiser,AdvertiserManager,etc though only the stats are loaded.)
                    }
                }
            }

            End();
        }
    }
}
