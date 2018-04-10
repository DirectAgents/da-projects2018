using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AdRoll;
using CakeExtracter.CakeMarketingApi.Entities;
using System;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AmazonDailySummaryLoader : Loader<AmazonDailySummary>
    {
        private readonly int accountId;
        private Dictionary<string, int> adIdLookupByEid = new Dictionary<string, int>();

        public AmazonDailySummaryLoader(int advertisableId)
        {
            this.accountId = advertisableId;
        }

        protected override int Load(List<AmazonDailySummary> items)
        {
            Logger.Info("Loading {0} AmazonDailySummaries..", items.Count);
          
            var count = UpsertDailySummaries(items);
            return count;
        }

        private int UpsertDailySummaries(List<AmazonDailySummary> items)
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
                    var source = new DirectAgents.Domain.Entities.CPProg.DailySummary
                    {
                        Date = item.date,
                        AccountId = accountId,
                        Impressions = item.impressions,
                        Clicks = item.clicks,
                        Cost = item.cost,
                        PostClickConv = item.attributedConversions14d,
                        PostClickRev = item.attributedSales14d
                    };
                    //var target = db.Set<DailySummary>().Find(DateTime.Now, accountId);
                    var target = (from p in db.DailySummaries
                                  where p.AccountId == accountId && p.Date == item.date
                                  select p).FirstOrDefault();
                    
                    if (target == null)
                    {
                        #region new data
                        if (!item.AllZeros())
                        {
                            db.DailySummaries.Add(source);
                            addedCount++;
                        }
                        else
                            alreadyDeletedCount++; 
                        #endregion
                    }
                    else // DailySummary already exists
                    {
                        #region already exists
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
                            {
                                entry.State = EntityState.Deleted;
                                deletedCount++;
                            }
                        }
                        else
                        {
                            duplicateCount++;
                        } 
                        #endregion
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
