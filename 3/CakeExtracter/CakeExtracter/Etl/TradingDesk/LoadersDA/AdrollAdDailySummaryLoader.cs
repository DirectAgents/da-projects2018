using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AdRoll;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollAdDailySummaryLoader : Loader<AdSummary>
    {
        private readonly int advertisableId;
        private Dictionary<string, int> adIdLookupByEid = new Dictionary<string, int>();

        public AdrollAdDailySummaryLoader(int advertisableId)
        {
            this.advertisableId = advertisableId;
        }

        protected override int Load(List<AdSummary> items)
        {
            Logger.Info("Loading {0} Adroll AdDailySummaries..", items.Count);
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
            var deletedCount = 0;
            var alreadyDeletedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    if (adIdLookupByEid.ContainsKey(item.eid))
                    {
                        int adId = adIdLookupByEid[item.eid];
                        if (!item.AllZeros(includeProspects: true))
                        {
                            var source = new AdDailySummary
                            {
                                Date = item.date,
                                AdId = adId,
                                Impressions = item.impressions,
                                Clicks = item.clicks,
                                CTC = item.click_through_conversions,
                                VTC = item.view_through_conversions,
                                Cost = (decimal)item.cost,
                                Prospects = item.prospects
                            };
                            var target = db.Set<AdDailySummary>().Find(item.date, adId);
                            if (target == null)
                            {
                                db.AdRollAdDailySummaries.Add(source);
                                addedCount++;
                            }
                            else
                            {
                                var entry = db.Entry(target);
                                if (entry.State == EntityState.Unchanged)
                                {
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
                        else // AllZeros...
                        {
                            var existing = db.Set<AdDailySummary>().Find(item.date, adId);
                            if (existing == null)
                                alreadyDeletedCount++;
                            else
                            {
                                db.AdRollAdDailySummaries.Remove(existing);
                                deletedCount++;
                            }
                        }
                    }
                    else // the eid was not in the Ad dictionary
                    {
                        skippedCount++;
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} AdRoll AdDailySummaries ({1} updates, {2} additions, {3} deletions, {4} already-deleted, {5} duplicates, {6} skipped)", itemCount, updatedCount, addedCount, deletedCount, alreadyDeletedCount, duplicateCount, skippedCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        private void AddUpdateDependentAds(List<AdSummary> items)
        {
            using (var db = new ClientPortalProgContext())
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
                        var ad = new DirectAgents.Domain.Entities.AdRoll.Ad
                        {
                            AdvertisableId = this.advertisableId,
                            Eid = group.Key.eid,
                            Name = group.Key.ad,

                            Type = groupAd.type,
                            CreatedDate = groupAd.created_date,
                            Width = groupAd.width,
                            Height = groupAd.height
                        };
                        db.AdRollAds.Add(ad);
                        db.SaveChanges();
                        Logger.Info("Saved new AdRoll Ad: {0} ({1}), Eid={2}", ad.Name, ad.Id, ad.Eid);
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
                            Logger.Info("Updated AdRoll Ad: {0}, Eid={1}", group.Key.ad, group.Key.eid);
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
