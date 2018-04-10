using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;
using FacebookAPI.Entities;

namespace CakeExtracter.Etl.SocialMarketing.LoadersDA
{
    public class FacebookDailySummaryLoader : Loader<FBSummary>
    {
        private readonly int accountId;

        public FacebookDailySummaryLoader(int accountId)
        {
            this.BatchSize = FacebookUtility.RowsReturnedAtATime; //FB API only returns 25 rows at a time
            this.accountId = accountId;
        }

        protected override int Load(List<FBSummary> items)
        {
            Logger.Info("Loading {0} Facebook DailySummaries..", items.Count);
            var count = UpsertDailySummaries(items);
            return count;
        }

        private int UpsertDailySummaries(List<FBSummary> items)
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
                    var source = new DailySummary
                    {
                        AccountId = accountId,
                        Date = item.Date,
                        Impressions = item.Impressions,
                        Clicks = item.LinkClicks,
                        AllClicks = item.AllClicks,
                        PostClickConv = item.Conversions_click,
                        PostViewConv = item.Conversions_view,
                        PostClickRev = item.ConVal_click,
                        PostViewRev = item.ConVal_view,
                        Cost = item.Spend
                    };
                    var target = db.Set<DailySummary>().Find(item.Date, accountId);
                    if (target == null)
                    {
                        if (!item.AllZeros())
                        {
                            db.DailySummaries.Add(source);
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
                                AutoMapper.Mapper.Map(source, target);
                                entry.State = EntityState.Modified;
                                updatedCount++;
                            }
                            else
                                entry.State = EntityState.Deleted;
                        }
                        else
                        {
                            Logger.Warn("Encountered duplicate for {0:d} - Acct {1}", item.Date, accountId);
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
