using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollAdSummaryLoader : Loader<AdSummary>
    {
        private readonly int accountId;
        private Dictionary<string, int> TDadIdLookupByEid = new Dictionary<string, int>();

        public string[] AdEids
        {
            get { return TDadIdLookupByEid.Keys.ToArray(); }
        }

        public AdrollAdSummaryLoader(int acctId)
        {
            this.accountId = acctId;
        }

        protected override int Load(List<AdSummary> items)
        {
            Logger.Info("Loading {0} TDadSummaries..", items.Count);
            AddUpdateDependentAds(items);
            var count = UpsertAdSummaries(items);
            return count;
        }

        private int UpsertAdSummaries(List<AdSummary> items)
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
                    if (TDadIdLookupByEid.ContainsKey(item.eid))
                    {
                        int TDadId = TDadIdLookupByEid[item.eid];
                        if (!item.AllZeros(includeProspects: false))
                        {
                            var source = new TDadSummary
                            {
                                Date = item.date,
                                TDadId = TDadId,
                                Impressions = item.impressions,
                                Clicks = item.clicks,
                                PostClickConv = item.click_through_conversions,
                                PostViewConv = item.view_through_conversions,
                                Cost = (decimal)item.cost,
                            };
                            var target = db.Set<TDadSummary>().Find(item.date, TDadId);
                            if (target == null)
                            { // add new
                                db.TDadSummaries.Add(source);
                                addedCount++;
                            }
                            else
                            { // update existing
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
                            var existing = db.Set<AdDailySummary>().Find(item.date, TDadId);
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
                Logger.Info("Saving {0} TDadSummaries ({1} updates, {2} additions, {3} deletions, {4} already-deleted, {5} duplicates, {6} skipped)", itemCount, updatedCount, addedCount, deletedCount, alreadyDeletedCount, duplicateCount, skippedCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        private void AddUpdateDependentAds(List<AdSummary> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                var tdAdsForAccount = db.TDads.Where(a => a.AccountId == this.accountId);

                // Find the unique Ads by grouping
                var itemGroups = items.GroupBy(i => new { i.eid, i.ad });
                foreach (var group in itemGroups)
                {
                    if (TDadIdLookupByEid.ContainsKey(group.Key.eid))
                        continue; // already encountered this eid

                    // See if a TDad with that externalId exists
                    var tdAdsInDb = tdAdsForAccount.Where(a => a.ExternalId == group.Key.eid);

                    //// If not, check by name
                    //if (adsInDb.Count() == 0)
                    //    adsInDb = db.AdRollAds.Where(a => a.Name == group.Key.ad);

                    // Assume all ads in the group have the same properties (just different dates/stats)
                    var groupAd = group.First();

                    if (tdAdsInDb.Count() == 0)
                    {   // TDad doesn't exist in the db; so create it and put an entry in the lookup
                        var tdAd = new TDad
                        {
                            AccountId = this.accountId,
                            ExternalId = group.Key.eid,
                            Name = group.Key.ad,
                            Width = groupAd.width,
                            Height = groupAd.height
                            //Type = groupAd.type,
                            //CreatedDate = groupAd.created_date,
                        };
                        db.TDads.Add(tdAd);
                        db.SaveChanges();
                        Logger.Info("Saved new TDad: {0} ({1}), ExternalId={2}", tdAd.Name, tdAd.Id, tdAd.ExternalId);
                        TDadIdLookupByEid[tdAd.ExternalId] = tdAd.Id;
                    }
                    else
                    {   // Update & put existing TDad in the lookup
                        // There should only be one matching TDad in the db, but just in case...
                        foreach (var tdAd in tdAdsInDb)
                        {
                            tdAd.ExternalId = group.Key.eid;
                            tdAd.Name = group.Key.ad;
                            tdAd.Width = groupAd.width;
                            tdAd.Height = groupAd.height;
                            //tdAd.Type = groupAd.type;
                            //tdAd.CreatedDate = groupAd.created_date;
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated TDad: {0}, ExternalId={1}", group.Key.ad, group.Key.eid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        TDadIdLookupByEid[group.Key.eid] = tdAdsInDb.First().Id;
                    }
                }
            }
        }
    }
}
