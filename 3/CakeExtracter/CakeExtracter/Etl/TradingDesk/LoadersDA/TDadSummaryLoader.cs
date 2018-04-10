using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDadSummaryLoader : Loader<TDadSummary>
    {
        private readonly int accountId; // only used in AddUpdateDependentTDads()
        private Dictionary<string, int> tdAdIdLookupByEidAndName = new Dictionary<string, int>();

        public TDadSummaryLoader(int accountId = -1)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<TDadSummary> items)
        {
            Logger.Info("Loading {0} DA-TD AdSummaries..", items.Count);
            AddUpdateDependentTDads(items);
            AssignTDadIdToItems(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        public void AssignTDadIdToItems(List<TDadSummary> items)
        {
            foreach (var item in items)
            {
                var eidAndName = item.TDadEid + item.TDadName;
                if (tdAdIdLookupByEidAndName.ContainsKey(eidAndName))
                {
                    item.TDadId = tdAdIdLookupByEidAndName[eidAndName];
                }
                // otherwise it will get skipped; no TDad to use for the foreign key
            }
        }

        public int UpsertDailySummaries(List<TDadSummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var deletedCount = 0;
            var alreadyDeletedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                var itemTDadIds = items.Select(i => i.TDadId).Distinct().ToArray();
                var tdAdIdsInDb = db.TDads.Select(a => a.Id).Where(i => itemTDadIds.Contains(i)).ToArray();

                foreach (var item in items)
                {
                    var target = db.Set<TDadSummary>().Find(item.Date, item.TDadId);
                    if (target == null)
                    {
                        if (item.AllZeros())
                        {
                            alreadyDeletedCount++;
                        }
                        else
                        {
                            if (tdAdIdsInDb.Contains(item.TDadId))
                            {
                                db.TDadSummaries.Add(item);
                                addedCount++;
                            }
                            else
                            {
                                Logger.Warn("Skipping load of item. TDad with id {0} does not exist.", item.TDadId);
                                skippedCount++;
                            }
                        }
                    }
                    else // TDadSummary already exists
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            if (!item.AllZeros())
                            {
                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(item, target);
                                entry.State = EntityState.Modified;
                                updatedCount++;
                            }
                            else
                            {
                                entry.State = EntityState.Deleted;
                                deletedCount++;
                            }
                        }
                        else
                        {
                            Logger.Warn("Encountered duplicate for {0:d} - TDad {1}", item.Date, item.TDadId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} TDadSummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted, {6} skipped)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount, skippedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        public void AddUpdateDependentTDads(List<TDadSummary> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                // Find the unique TDads by grouping
                var itemGroups = items.GroupBy(i => new { i.TDadName, i.TDadEid });
                foreach (var group in itemGroups)
                {
                    string eidAndName = group.Key.TDadEid + group.Key.TDadName;
                    if (tdAdIdLookupByEidAndName.ContainsKey(eidAndName))
                        continue; // already encountered this TDad

                    IQueryable<TDad> tdAdsInDb = null;
                    if (!string.IsNullOrWhiteSpace(group.Key.TDadEid))
                    {
                        // See if a TDad with that ExternalId exists
                        tdAdsInDb = db.TDads.Where(a => a.AccountId == accountId && a.ExternalId == group.Key.TDadEid);
                        //if (!tdAdsInDb.Any()) // If not, check for a match by name where ExternalId == null
                        //    tdAdsInDb = db.TDads.Where(x => x.AccountId == accountId && x.ExternalId == null && x.Name == group.Key.TDadName);
                    }
                    else
                    {
                        // Check by TDad name
                        tdAdsInDb = db.TDads.Where(s => s.AccountId == accountId && s.Name == group.Key.TDadName);
                    }
                    //Note: If we're grouping by ad name, then the ads in the db and the ads to-be-loaded shouldn't have externalIds filled in.

                    var tdAdsInDbList = tdAdsInDb.ToList();
                    if (!tdAdsInDbList.Any())
                    {   // TDad doesn't exist in the db; so create it and put an entry in the lookup
                        var tdAd = new TDad
                        {
                            AccountId = this.accountId,
                            ExternalId = group.Key.TDadEid,
                            Name = group.Key.TDadName
                            // other properties...
                        };
                        db.TDads.Add(tdAd);
                        db.SaveChanges();
                        Logger.Info("Saved new TDad: {0} ({1}), ExternalId={2}", tdAd.Name, tdAd.Id, tdAd.ExternalId);
                        tdAdIdLookupByEidAndName[eidAndName] = tdAd.Id;
                    }
                    else
                    {   // Update & put existing TDad in the lookup
                        // There should only be one matching TDad in the db, but just in case...
                        foreach (var tdAd in tdAdsInDbList)
                        {
                            if (!string.IsNullOrWhiteSpace(group.Key.TDadEid))
                                tdAd.ExternalId = group.Key.TDadEid;
                            if (!string.IsNullOrWhiteSpace(group.Key.TDadName))
                                tdAd.Name = group.Key.TDadName;
                            // other properties...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated TDad: {0}, Eid={1}", group.Key.TDadName, group.Key.TDadEid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        tdAdIdLookupByEidAndName[eidAndName] = tdAdsInDbList.First().Id;
                    }
                }
            }
        }

    }
}
