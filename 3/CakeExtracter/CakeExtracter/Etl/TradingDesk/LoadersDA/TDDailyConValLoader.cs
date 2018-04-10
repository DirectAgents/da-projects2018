using System.Collections.Generic;
using System.Data.Entity;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDDailyConValLoader : Loader<DailySummary>
    {
        private readonly int accountId;

        public TDDailyConValLoader(int accountId)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<DailySummary> items)
        {
            Logger.Info("Loading {0} DA-TD DailySummary ConVals..", items.Count);
            var count = UpsertDailySummaryConVals(items);
            return count;
        }

        public int UpsertDailySummaryConVals(List<DailySummary> items)
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
                    var target = db.Set<DailySummary>().Find(item.Date, accountId);
                    if (target == null)
                    {
                        if (item.PostClickRev != 0 || item.PostViewRev != 0)
                        {
                            var ds = new DailySummary
                            {
                                Date = item.Date,
                                AccountId = accountId,
                                PostClickRev = item.PostClickRev,
                                PostViewRev = item.PostViewRev
                            };
                            db.DailySummaries.Add(ds);
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
                            target.PostClickRev = item.PostClickRev;
                            target.PostViewRev = item.PostViewRev;
                            updatedCount++;
                        }
                        else
                        {
                            Logger.Warn("Encountered duplicate for {0:d} - Acct {1}", item.Date, item.AccountId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} DailySummary ConVals ({1} updates, {2} additions, {3} duplicates, {4} already-deleted)",
                            itemCount, updatedCount, addedCount, duplicateCount, alreadyDeletedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

    }
}
