using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDDailySummaryLoader : Loader<DailySummary>
    {
        private readonly int accountId;

        public TDDailySummaryLoader(int accountId = -1)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<DailySummary> items)
        {
            Logger.Info("Loading {0} DA-TD DailySummaries..", items.Count);
            AssignIdsToItems(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        public void AssignIdsToItems(List<DailySummary> items)
        {
            foreach (var item in items)
            {
                item.AccountId = accountId;
            }
        }

        public int UpsertDailySummaries(List<DailySummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var deletedCount = 0;
            var alreadyDeletedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    var target = db.Set<DailySummary>().Find(item.Date, item.AccountId);
                    if (target == null)
                    {
                        if (!item.AllZeros())
                        {
                            //db.DailySummaries.Add(source);
                            db.DailySummaries.Add(item);
                            addedCount++;
                        }
                        else
                            alreadyDeletedCount++;
                    }
                    else // DailySummary already exists
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            if (!item.AllZeros())
                            {
                                entry.State = EntityState.Detached;
                                //AutoMapper.Mapper.Map(source, target);
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
                            Logger.Warn("Encountered duplicate for {0:d} - Acct {1}", item.Date, item.AccountId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} DailySummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }
    }
}
