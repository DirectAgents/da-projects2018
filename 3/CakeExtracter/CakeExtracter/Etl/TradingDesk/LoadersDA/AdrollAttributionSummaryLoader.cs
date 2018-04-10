using System.Collections.Generic;
using System.Data.Entity;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollAttributionSummaryLoader : Loader<AttributionSummary>
    {
        private readonly int accountId;

        public AdrollAttributionSummaryLoader(int accountId)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<AttributionSummary> items)
        {
            Logger.Info("Loading {0} AdRoll AttributionSummaries..", items.Count);
            var count = UpsertAttributionSummaries(items);
            return count;
        }

        private int UpsertAttributionSummaries(List<AttributionSummary> items)
        {
            var updatedCount = 0;
            var duplicateCount = 0;
            var missingCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    var dSum = db.Set<DailySummary>().Find(item.date, accountId);
                    if (dSum == null)
                    {
                        missingCount++;
                    }
                    else
                    {
                        var entry = db.Entry(dSum);
                        if (entry.State == EntityState.Unchanged)
                        {
                            //dSum.PostClickConv = item.click_throughs; // already set
                            //dSum.PostViewConv = item.view_throughs; // already set
                            dSum.PostClickRev = (decimal)item.click_revenue;
                            dSum.PostViewRev = (decimal)item.view_revenue;
                            updatedCount++;
                        }
                        else
                        {
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Updating {0} AttributionSummaries. {1} not found. {2} duplicates.", updatedCount, missingCount, duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }
    }
}
