using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDStrategyConValLoader : Loader<StrategySummary>
    {
        private readonly int accountId;
        private Dictionary<string, int> strategyIdLookupByEidAndName = new Dictionary<string, int>();

        public TDStrategyConValLoader(int accountId)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<StrategySummary> items)
        {
            Logger.Info("Loading {0} DA-TD StrategySummary ConVals..", items.Count);
            AddUpdateDependentStrategies(items);
            AssignStrategyIdToItems(items);
            var count = UpsertStrategySummaryConVals(items);
            return count;
        }

        public void AddUpdateDependentStrategies(List<StrategySummary> items)
        {
            var strategyNameEids = items.GroupBy(i => new { i.StrategyName, i.StrategyEid })
                .Select(g => new NameEid { Name = g.Key.StrategyName, Eid = g.Key.StrategyEid });
            TDStrategySummaryLoader.AddUpdateDependentStrategies(strategyNameEids, this.accountId, this.strategyIdLookupByEidAndName);
        }
        public void AssignStrategyIdToItems(List<StrategySummary> items)
        {
            TDStrategySummaryLoader.AssignStrategyIdToItems(items, this.strategyIdLookupByEidAndName);
        }

        public int UpsertStrategySummaryConVals(List<StrategySummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var alreadyDeletedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    var target = db.Set<StrategySummary>().Find(item.Date, item.StrategyId);
                    if (target == null)
                    {
                        if (item.PostClickRev != 0 || item.PostViewRev != 0)
                        {
                            var ds = new StrategySummary
                            {
                                Date = item.Date,
                                StrategyId = item.StrategyId,
                                PostClickRev = item.PostClickRev,
                                PostViewRev = item.PostViewRev
                            };
                            db.StrategySummaries.Add(ds);
                            addedCount++;
                        }
                        else
                            alreadyDeletedCount++;
                    }
                    else // StrategySummary already exists
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            target.PostClickRev = item.PostClickRev;
                            target.PostViewRev = item.PostViewRev;
                            updatedCount++;
                        }
                        else
                        {
                            Logger.Warn("Encountered duplicate for {0:d} - StrategyId {1}", item.Date, item.StrategyId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} StrategySummary ConVals ({1} updates, {2} additions, {3} duplicates, {4} already-deleted)",
                            itemCount, updatedCount, addedCount, duplicateCount, alreadyDeletedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

    }
}
