using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollDailySummaryLoader : Loader<AdrollDailySummary>
    {
        private readonly int accountId;

        public AdrollDailySummaryLoader(string advertisableEid)
        {
            using (var db = new ClientPortalProgContext())
            {
                var account = db.ExtAccounts.Where(a => a.ExternalId == advertisableEid && a.Platform.Code == Platform.Code_AdRoll)
                                            .FirstOrDefault();
                if (account != null)
                    accountId = account.Id;
                else
                    accountId = -1;
            }
        }

        public bool FoundAccount()
        {
            return (accountId > -1);
        }
        public int AccountId
        {
            get { return accountId; }
        }

        protected override int Load(List<AdrollDailySummary> items)
        {
            Logger.Info("Loading {0} AdRoll DailySummaries..", items.Count);
            var count = UpsertDailySummaries(items);
            return count;
        }

        private int UpsertDailySummaries(List<AdrollDailySummary> items)
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
                        Date = item.date,
                        AccountId = accountId,
                        Impressions = item.impressions,
                        Clicks = item.clicks,
                        PostClickConv = item.click_through_conversions,
                        PostViewConv = item.view_through_conversions,
                        Cost = (decimal)item.cost
                    };
                    var target = db.Set<DailySummary>().Find(item.date, accountId);
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
                                //Preserve these. (They're updated in the AdrollAttributionSummaryLoader.)
                                var postClickRev = target.PostClickRev;
                                var postViewRev = target.PostViewRev;

                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(source, target);

                                target.PostClickRev = postClickRev;
                                target.PostViewRev = postViewRev;

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
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} DailySummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

    }
}
