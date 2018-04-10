using System;
using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class CampaignSummaryLoader : Loader<CampaignSummary>
    {
        private readonly DateTime date;

        // to keep track of offId/affIds loaded (and if more than once)
        private Dictionary<Tuple<int, int>, int> offAffDict = new Dictionary<Tuple<int, int>, int>();

        public CampaignSummaryLoader(DateTime date)
        {
            this.date = date;
        }

        public Dictionary<Tuple<int, int>, int> GetLoadedOffAffs()
        {
            return offAffDict;
        }

        protected override int Load(List<CampaignSummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
            {
                foreach (var item in items)
                {
                    var pk1 = date;
                    var pk2 = item.SiteOffer.SiteOfferId;
                    var pk3 = item.SourceAffiliate.SourceAffiliateId;
                    var target = db.Set<ClientPortal.Data.Contexts.DailySummary>().Find(pk1, pk2, pk3);

                    if (target == null)
                    {
                        target = new ClientPortal.Data.Contexts.DailySummary
                        {
                            Date = pk1,
                            OfferId = pk2,
                            AffiliateId = pk3
                        };
                        db.DailySummaries.Add(target);
                        added++;
                    }
                    else
                    {
                        updated++;
                    }

                    target.Views = item.Views;
                    target.Clicks = item.Clicks;
                    target.Conversions = Convert.ToInt32(item.MacroEventConversions);
                    target.Paid = Convert.ToInt32(item.Paid);
                    target.Sellable = Convert.ToInt32(item.Sellable);
                    target.Cost = item.Cost;
                    target.Revenue = item.Revenue;

                    // Record the fact that stats for this offer/affiliate were loaded.
                    var offAffTuple = new Tuple<int,int>(item.SiteOffer.SiteOfferId, item.SourceAffiliate.SourceAffiliateId);
                    if (offAffDict.ContainsKey(offAffTuple))
                    {
                        offAffDict[offAffTuple]++;
                        Logger.Info("DailySummary for {0:d}, OffId {1}, AffId {2} updated more than once in loader",
                                    date, offAffTuple.Item1, offAffTuple.Item2);
                    }
                    else
                        offAffDict.Add(offAffTuple, 1);

                    loaded++;
                }
                Logger.Info("Loading {0} DailySummaries ({1} updates, {2} additions)", loaded, updated, added);

                db.SaveChanges();
            }
            return loaded;
        }
    }
}
