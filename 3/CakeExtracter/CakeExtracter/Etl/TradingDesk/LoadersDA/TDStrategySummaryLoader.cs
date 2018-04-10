using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDStrategySummaryLoader : Loader<StrategySummary>
    {
        // Note accountId is only used in AddUpdateDependentStrategies() (i.e. not by AdrollCampaignSummaryLoader)
        private readonly int accountId;
        private Dictionary<string, int> strategyIdLookupByEidAndName = new Dictionary<string, int>();
        private bool preLoadStrategies;

        public TDStrategySummaryLoader(int accountId = -1, bool preLoadStrategies = false)
        {
            this.accountId = accountId;
            this.preLoadStrategies = preLoadStrategies;
        }

        protected override int Load(List<StrategySummary> items)
        {
            Logger.Info("Loading {0} DA-TD StrategySummaries..", items.Count);
            if (!preLoadStrategies)
                AddUpdateDependentStrategies(items);
            AssignStrategyIdToItems(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        public void AssignStrategyIdToItems(List<StrategySummary> items)
        {
            AssignStrategyIdToItems(items, this.strategyIdLookupByEidAndName);
        }
        public static void AssignStrategyIdToItems(List<StrategySummary> items, Dictionary<string, int> strategyIdLookupByEidAndName)
        {
            foreach (var item in items)
            {
                var eidAndName = item.StrategyEid + item.StrategyName;
                if (strategyIdLookupByEidAndName.ContainsKey(eidAndName))
                {
                    item.StrategyId = strategyIdLookupByEidAndName[eidAndName];
                }
                // otherwise it will get skipped; no strategy to use for the foreign key
            }
        }

        public int UpsertDailySummaries(List<StrategySummary> items)
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
                var itemStrategyIds = items.Select(i => i.StrategyId).Distinct().ToArray();
                var strategyIdsInDb = db.Strategies.Select(s => s.Id).Where(i => itemStrategyIds.Contains(i)).ToArray();

                foreach (var item in items)
                {
                    var target = db.Set<StrategySummary>().Find(item.Date, item.StrategyId);
                    if (target == null)
                    {
                        if (item.AllZeros())
                        {
                            alreadyDeletedCount++;
                        }
                        else
                        {
                            if (strategyIdsInDb.Contains(item.StrategyId))
                            {
                                db.StrategySummaries.Add(item);
                                addedCount++;
                            }
                            else
                            {
                                Logger.Warn("Skipping load of item. Strategy with id {0} does not exist.", item.StrategyId);
                                skippedCount++;
                            }
                        }
                    }
                    else // StrategySummary already exists
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
                            Logger.Warn("Encountered duplicate for {0:d} - Strategy {1}", item.Date, item.StrategyId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} StrategySummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted, {6} skipped)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount, skippedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        public void AddUpdateDependentStrategies(List<StrategySummary> items)
        {
            var strategyNameEids = items.GroupBy(i => new { i.StrategyName, i.StrategyEid })
                .Select(g => new NameEid { Name = g.Key.StrategyName, Eid = g.Key.StrategyEid });
            AddUpdateDependentStrategies(strategyNameEids, this.accountId, this.strategyIdLookupByEidAndName);
        }
        public void AddUpdateDependentStrategies(IEnumerable<NameEid> strategyNameEids)
        {
            AddUpdateDependentStrategies(strategyNameEids, this.accountId, this.strategyIdLookupByEidAndName);
        }
        public static void AddUpdateDependentStrategies(IEnumerable<NameEid> strategyNameEids, int accountId, Dictionary<string, int> strategyIdLookupByEidAndName)
        {
            using (var db = new ClientPortalProgContext())
            {
                foreach (var nameEid in strategyNameEids)
                {
                    string eidAndName = nameEid.Eid + nameEid.Name;
                    if (strategyIdLookupByEidAndName.ContainsKey(eidAndName))
                        continue; // already encountered this strategy

                    IQueryable<Strategy> stratsInDb = null;
                    if (!string.IsNullOrWhiteSpace(nameEid.Eid))
                    {
                        // First see if a Strategy with that ExternalId exists
                        stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.ExternalId == nameEid.Eid);
                        if (!stratsInDb.Any()) // If not, check for a match by name where ExternalId == null
                            stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.ExternalId == null && s.Name == nameEid.Name);
                    }
                    else
                    {
                        // Check by strategy name
                        stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.Name == nameEid.Name);
                    }

                    var stratsInDbList = stratsInDb.ToList();
                    if (!stratsInDbList.Any())
                    {   // Strategy doesn't exist in the db; so create it and put an entry in the lookup
                        var strategy = new Strategy
                        {
                            AccountId = accountId,
                            ExternalId = nameEid.Eid,
                            Name = nameEid.Name
                            // other properties...
                        };
                        db.Strategies.Add(strategy);
                        db.SaveChanges();
                        Logger.Info("Saved new Strategy: {0} ({1}), ExternalId={2}", strategy.Name, strategy.Id, strategy.ExternalId);
                        strategyIdLookupByEidAndName[eidAndName] = strategy.Id;
                    }
                    else
                    {   // Update & put existing Strategy in the lookup
                        // There should only be one matching Strategy in the db, but just in case...
                        foreach (var strat in stratsInDbList)
                        {
                            if (!string.IsNullOrWhiteSpace(nameEid.Eid))
                                strat.ExternalId = nameEid.Eid;
                            if (!string.IsNullOrWhiteSpace(nameEid.Name))
                                strat.Name = nameEid.Name;
                            // other properties...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated Strategy: {0}, Eid={1}", nameEid.Name, nameEid.Eid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        strategyIdLookupByEidAndName[eidAndName] = stratsInDbList.First().Id;
                    }
                }
            }
        }

    }
    public class NameEid
    {
        public string Name { get; set; }
        public string Eid { get; set; }
    }
}
