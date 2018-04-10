using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AdRoll.Entities;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.AdRoll;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class AdrollAdDailySummaryLoader2 : Loader<AdSummary>
    {
        private readonly int adrollProfileId;
        private Dictionary<string, int> adIdLookupByEid = new Dictionary<string, int>();

        public AdrollAdDailySummaryLoader2(int adrollProfileId)
        {
            this.adrollProfileId = adrollProfileId;
        }

        protected override int Load(List<AdSummary> items)
        {
            Logger.Info("Loading {0} AdrollAdDailySummaries..", items.Count);
            AddUpdateDependentAds(items);
            var count = UpsertAdDailySummaries(items);
            return count;
        }

        private int UpsertAdDailySummaries(List<AdSummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach (var item in items)
                {
                    if (adIdLookupByEid.ContainsKey(item.eid))
                    {
                        int adId = adIdLookupByEid[item.eid];
                        var source = new AdDailySummary
                        {
                            Date = item.date,
                            AdRollAdId = adId,
                            Impressions = item.impressions,
                            Clicks = item.clicks,
                            //Conversions = item.total_conversions,
                            Conversions = item.click_through_conversions + item.view_through_conversions,
                            Spend = (decimal)item.cost
                        };
                        var target = db.Set<AdDailySummary>().Find(item.date, adId);
                        if (target == null)
                        {
                            db.AdDailySummaries.Add(source);
                            addedCount++;
                        }
                        else
                        {
                            var entry = db.Entry(target);
                            if (entry.State == EntityState.Unchanged)
                            {
                                //TODO:
                                //1) check if anything changed. if not, set Unchanged
                                //2) see what sql statements are actually executed

                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(source, target);
                                entry.State = EntityState.Modified;
                                updatedCount++;
                            }
                            else
                            {
                                duplicateCount++;
                            }
                        }
                    }
                    else
                    {
                        skippedCount++;
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} AdDailySummaries ({1} updates, {2} additions, {3} duplicates, {4} skipped)", itemCount, updatedCount, addedCount, duplicateCount, skippedCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        private void AddUpdateDependentAds(List<AdSummary> items)
        {
            using (var db = new TDContext())
            {
                // Find the unique Ads by grouping
                var itemGroups = items.GroupBy(i => new { i.eid, i.ad });
                foreach (var group in itemGroups)
                {
                    if (adIdLookupByEid.ContainsKey(group.Key.eid))
                        continue; // already encountered this eid

                    // See if an AdRollAd with that eid exists
                    var adsInDb = db.AdRollAds.Where(a => a.Eid == group.Key.eid);

                    //// If not, check by name
                    //if (adsInDb.Count() == 0)
                    //    adsInDb = db.AdRollAds.Where(a => a.Name == group.Key.ad);

                    // Assume all ads in the group have the same properties (just different dates/stats)
                    var groupAd = group.First();

                    if (adsInDb.Count() == 0)
                    {   // Ad doesn't exist in the db; so create it and put an entry in the lookup
                        var ad = new AdRollAd
                        {
                            AdRollProfileId = adrollProfileId,
                            Eid = group.Key.eid,
                            Name = group.Key.ad,

                            Type = groupAd.type,
                            CreatedDate = groupAd.created_date,
                            Width = groupAd.width,
                            Height = groupAd.height
                        };
                        db.AdRollAds.Add(ad);
                        db.SaveChanges();
                        Logger.Info("Saved new AdRollAd: {0} ({1}), Eid={2}", ad.Name, ad.Id, ad.Eid);
                        adIdLookupByEid[ad.Eid] = ad.Id;
                    }
                    else
                    {   // Update & put existing Ad in the lookup
                        // There should only be one matching Ad in the db, but just in case...
                        foreach (var ad in adsInDb)
                        {
                            ad.Eid = group.Key.eid;
                            ad.Name = group.Key.ad;

                            ad.Width = groupAd.width;
                            ad.Height = groupAd.height;
                            ad.Type = groupAd.type;
                            ad.CreatedDate = groupAd.created_date;
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated AdRollAd: {0}, Eid={1}", group.Key.ad, group.Key.eid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        adIdLookupByEid[group.Key.eid] = adsInDb.First().Id;
                    }
                }
            }
        }
    }
}
