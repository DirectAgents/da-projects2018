using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDAdSetSummaryLoader : Loader<AdSetSummary>
    {
        private readonly int accountId;
        private Dictionary<string, int> strategyIdLookup = new Dictionary<string, int>(); // by StrategyEid + StrategyName
        private Dictionary<string, int> adsetIdLookup = new Dictionary<string, int>(); // by StrategyEid + StrategyName + AdSetEid + AdSetName

        public TDAdSetSummaryLoader(int accountId = -1)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<AdSetSummary> items)
        {
            Logger.Info("Loading {0} DA-TD AdSetSummaries..", items.Count);
            AddUpdateDependentStrategies(items);
            AddUpdateDependentAdSets(items);
            AssignAdSetIdToItems(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        public void AssignAdSetIdToItems(List<AdSetSummary> items)
        {
            foreach (var item in items)
            {
                string adsetKey = item.StrategyEid + item.StrategyName + item.AdSetEid + item.AdSetName;
                if (adsetIdLookup.ContainsKey(adsetKey))
                {
                    item.AdSetId = adsetIdLookup[adsetKey];
                }
                // otherwise it will get skipped; no AdSet to use for the foreign key
            }
        }

        public int UpsertDailySummaries(List<AdSetSummary> items)
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
                var itemAdSetIds = items.Select(i => i.AdSetId).Distinct().ToArray();
                var adsetIdsInDb = db.AdSets.Select(s => s.Id).Where(i => itemAdSetIds.Contains(i)).ToArray();
                foreach (var item in items)
                {
                    var target = db.Set<AdSetSummary>().Find(item.Date, item.AdSetId);
                    if (target == null)
                    {
                        if (item.AllZeros())
                        {
                            alreadyDeletedCount++;
                        }
                        else
                        {
                            if (adsetIdsInDb.Contains(item.AdSetId))
                            {
                                db.AdSetSummaries.Add(item);
                                addedCount++;
                            }
                            else
                            {
                                Logger.Warn("Skipping load of item. AdSet with id {0} does not exist.", item.AdSetId);
                                skippedCount++;
                            }
                        }
                    }
                    else // AdSetSummary already exists
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
                            Logger.Warn("Encountered duplicate for {0:d} - AdSet {1}", item.Date, item.AdSetId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} AdSetSummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted, {6} skipped)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount, skippedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        public void AddUpdateDependentStrategies(List<AdSetSummary> items)
        {
            var strategyNameEids = items.GroupBy(i => new { i.StrategyName, i.StrategyEid })
                .Select(g => new NameEid { Name = g.Key.StrategyName, Eid = g.Key.StrategyEid })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Eid));
            TDStrategySummaryLoader.AddUpdateDependentStrategies(strategyNameEids, this.accountId, this.strategyIdLookup);
        }

        public void AddUpdateDependentAdSets(List<AdSetSummary> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                // Find the unique adsets by grouping
                var itemGroups = items.GroupBy(i => new { i.StrategyName, i.StrategyEid, i.AdSetName, i.AdSetEid });
                foreach (var group in itemGroups)
                {
                    string adsetKey = group.Key.StrategyEid + group.Key.StrategyName + group.Key.AdSetEid + group.Key.AdSetName;
                    if (adsetIdLookup.ContainsKey(adsetKey))
                        continue; // already encountered this adset

                    var stratKey = group.Key.StrategyEid + group.Key.StrategyName;
                    int? stratId = strategyIdLookup.ContainsKey(stratKey) ? strategyIdLookup[stratKey] : (int?)null;

                    //TODO: Check this logic for finding an existing adset...
                    //      The main concern is when uploading stats from a csv and AdSetEids aren't included

                    IQueryable<AdSet> adsetsInDb = null;
                    if (!string.IsNullOrWhiteSpace(group.Key.AdSetEid))
                    {
                        // First see if an AdSet with that ExternalId exists
                        adsetsInDb = db.AdSets.Where(x => x.AccountId == accountId && x.ExternalId == group.Key.AdSetEid);

                        // If not, check for a match by name where ExternalId == null
                        if (!adsetsInDb.Any())
                        {
                            adsetsInDb = db.AdSets.Where(x => x.AccountId == accountId && x.ExternalId == null && x.Name == group.Key.AdSetName);
                            if (stratId.HasValue)
                                adsetsInDb = adsetsInDb.Where(x => x.StrategyId == stratId.Value);
                        }
                    }
                    else
                    {
                        // Check by adset name
                        adsetsInDb = db.AdSets.Where(x => x.AccountId == accountId && x.Name == group.Key.AdSetName);
                        if (stratId.HasValue)
                            adsetsInDb = adsetsInDb.Where(x => x.StrategyId == stratId.Value);
                    }

                    var adsetsInDbList = adsetsInDb.ToList();
                    if (!adsetsInDbList.Any())
                    {   // AdSet doesn't exist in the db; so create it and put an entry in the lookup
                        var adset = new AdSet
                        {
                            AccountId = this.accountId,
                            StrategyId = stratId,
                            ExternalId = group.Key.AdSetEid,
                            Name = group.Key.AdSetName
                        };
                        db.AdSets.Add(adset);
                        db.SaveChanges();
                        Logger.Info("Saved new AdSet: {0} ({1}), ExternalId={2}", adset.Name, adset.Id, adset.ExternalId);
                        adsetIdLookup[adsetKey] = adset.Id;
                    }
                    else
                    {   // Update & put existing AdSet in the lookup
                        // There should only be one matching AdSet in the db, but just in case...
                        foreach (var adset in adsetsInDbList)
                        {
                            if (!string.IsNullOrWhiteSpace(group.Key.AdSetEid))
                                adset.ExternalId = group.Key.AdSetEid;
                            if (!string.IsNullOrWhiteSpace(group.Key.AdSetName))
                                adset.Name = group.Key.AdSetName;
                            if (stratId.HasValue)
                                adset.StrategyId = stratId.Value;
                            // other properties...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated AdSet: {0}, Eid={1}", group.Key.AdSetName, group.Key.AdSetEid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        adsetIdLookup[adsetKey] = adsetsInDbList.First().Id;
                    }
                }
            }
        }

    }
}
