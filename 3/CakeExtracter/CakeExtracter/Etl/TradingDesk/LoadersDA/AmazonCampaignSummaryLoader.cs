using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Amazon.Entities;
using DirectAgents.Domain.Contexts;
using CakeExtracter.CakeMarketingApi.Entities;
using System;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AmazonCampaignSummaryLoader : Loader<StrategySummary>
    {
        //private readonly int accountId;
        //private Dictionary<string, int> strategyIdLookupByCampEid = new Dictionary<string, int>();

        //public AmazonCampaignSummaryLoader(int advertisableEid)
        //{
        //    using (var db = new ClientPortalProgContext())
        //    {
        //        var account = db.ExtAccounts.Where(a => a.ExternalId == advertisableEid.ToString() && a.Platform.Code == Platform.Code_Amazon)
        //                                    .FirstOrDefault();
        //        if (account != null)
        //            accountId = account.Id;
        //        else
        //            accountId = -1;

        //        this.strategySummaryLoader = new TDStrategySummaryLoader(accountId);
        //    }
        //}
        //public AmazonCampaignSummaryLoader(int advertisableId)
        //{
        //    this.accountId = advertisableId;
        //}
        //public bool FoundAccount()
        //{
        //    return (accountId > -1);
        //}

        //protected override int Load(List<StrategySummary> items)
        //{
        //    var count = items.Count;
        //    Logger.Info("Loading {0} Amazon CampaignDailySummaries..", items.Count);
        //    foreach (var item in items)
        //    {
        //        //item.eid = "2436984122296584";
        //    }
        //    AddUpdateDependentStrategies(items);
        //    var ssItems = items.Select(cSum => CreateStrategySummary(cSum, strategyIdLookupByCampEid[cSum.eid])).ToList();
        //    var count = strategySummaryLoader.UpsertCampaigns(items);
        //    return count;
        //}
        //public int UpsertCampaigns(List<AmazonCampaignSummary> items)
        //{
        //    var addedCount = 0;
        //    var updatedCount = 0;
        //    var duplicateCount = 0;
        //    var deletedCount = 0;
        //    var alreadyDeletedCount = 0;
        //    var skippedCount = 0;
        //    var itemCount = 0;
        //    using (var db = new ClientPortalProgContext())
        //    {
        //        var itemStrategyIds = items.Select(i => i.StrategyId).Distinct().ToArray();
        //        var strategyIdsInDb = db.Strategies.Select(s => s.Id).Where(i => itemStrategyIds.Contains(i)).ToArray();

        //        foreach (var item in items)
        //        {
        //            var target = db.Set<StrategySummary>().Find(item.Date, item.StrategyId);
        //            if (target == null)
        //            {
        //                if (item.AllZeros())
        //                {
        //                    alreadyDeletedCount++;
        //                }
        //                else
        //                {
        //                    if (strategyIdsInDb.Contains(item.StrategyId))
        //                    {
        //                        db.StrategySummaries.Add(item);
        //                        addedCount++;
        //                    }
        //                    else
        //                    {
        //                        Logger.Warn("Skipping load of item. Strategy with id {0} does not exist.", item.StrategyId);
        //                        skippedCount++;
        //                    }
        //                }
        //            }
        //            else // StrategySummary already exists
        //            {
        //                var entry = db.Entry(target);
        //                if (entry.State == EntityState.Unchanged)
        //                {
        //                    if (!item.AllZeros())
        //                    {
        //                        entry.State = EntityState.Detached;
        //                        AutoMapper.Mapper.Map(item, target);
        //                        entry.State = EntityState.Modified;
        //                        updatedCount++;
        //                    }
        //                    else
        //                    {
        //                        entry.State = EntityState.Deleted;
        //                        deletedCount++;
        //                    }
        //                }
        //                else
        //                {
        //                    Logger.Warn("Encountered duplicate for {0:d} - Strategy {1}", item.Date, item.StrategyId);
        //                    duplicateCount++;
        //                }
        //            }
        //            itemCount++;
        //        }
        //        Logger.Info("Saving {0} StrategySummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted, {6} skipped)",
        //                    itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount, skippedCount);
        //        if (duplicateCount > 0)
        //            Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
        //        int numChanges = db.SaveChanges();
        //    }
        //    return itemCount;
        //}

        //public static StrategySummary CreateStrategySummary(AmazonCampaignSummary cSum, int strategyId)
        //{
        //    var sSum = new StrategySummary
        //    {
        //        StrategyId = strategyId,
        //        //Date = cSum.date,
        //        //Impressions = cSum.impressions,
        //        //Clicks = cSum.clicks,
        //        //PostClickConv = cSum.click_through_conversions,
        //        //PostViewConv = cSum.view_through_conversions,
        //        PostClickRev = (decimal)cSum.adjusted_attributed_click_through_rev,
        //        PostViewRev = (decimal)cSum.adjusted_attributed_view_through_rev,
        //        //Cost = (decimal)cSum.cost
        //        // Prospects?
        //    };
        //    return sSum;
        //}

        //private void AddUpdateDependentStrategies(List<StrategySummary> items)
        //{
        //    using (var db = new ClientPortalProgContext())
        //    {
        //        // Find the unique campaigns by grouping
        //        var itemGroups = items.GroupBy(i => new { i.name, i.campaignId });
        //        foreach (var group in itemGroups)
        //        {
        //            if (strategyIdLookupByCampEid.ContainsKey(group.Key.name))
        //                continue; // already encountered this eid

        //            // See if a Strategy with that ExternalId exists
        //            var stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.Name == group.Key.name);

        //            //// If not, check by name
        //            //if (stratsInDb.Count() == 0)
        //            //    stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s => s.Name == group.Key.campaign);

        //            // Assume all strategies in the group have the same properties (just different dates/stats)
        //            //var groupStrat = group.First();

        //            if (!stratsInDb.Any())
        //            {   // Strategy doesn't exist in the db; so create it and put an entry in the lookup
        //                var strategy = new Strategy
        //                {
        //                    AccountId = this.accountId,
        //                    ExternalId = group.Key.campaignId.ToString(),
        //                    Name = group.Key.name
        //                    // type, status, created, start, end, etc...
        //                };
        //                db.Strategies.Add(strategy);
        //                db.SaveChanges();
        //                Logger.Info("Saved new Strategy: {0} ({1}), ExternalId={2}", strategy.Name, strategy.Id, strategy.ExternalId);
        //                strategyIdLookupByCampEid[strategy.ExternalId] = strategy.Id;
        //            }
        //            else
        //            {   // Update & put existing Strategy in the lookup
        //                // There should only be one matching Strategy in the db, but just in case...
        //                foreach (var strat in stratsInDb)
        //                {
        //                    strat.ExternalId = group.Key.campaignId.ToString();
        //                    strat.Name = group.Key.name;
        //                    // type, status, created, start, end, etc...
        //                }
        //                int numUpdates = db.SaveChanges();
        //                if (numUpdates > 0)
        //                {
        //                    Logger.Info("Updated Strategy: {0}, Eid={1}", group.Key.name, group.Key.campaignId.ToString());
        //                    if (numUpdates > 1)
        //                        Logger.Warn("Multiple entities in db ({0})", numUpdates);
        //                }
        //                strategyIdLookupByCampEid[group.Key.campaignId.ToString()] = stratsInDb.First().Id;
        //            }
        //        }
        //    }
        //}

        private TDStrategySummaryLoader tdStrategySummaryLoader;

        public AmazonCampaignSummaryLoader(int accountId = -1)
        {
            this.tdStrategySummaryLoader = new TDStrategySummaryLoader(accountId);
        }

        protected override int Load(List<StrategySummary> items)
        {
            Logger.Info("Loading {0} Amazon Campaign Daily Summaries..", items.Count);
            tdStrategySummaryLoader.AddUpdateDependentStrategies(items);
            tdStrategySummaryLoader.AssignStrategyIdToItems(items);
            var count = tdStrategySummaryLoader.UpsertDailySummaries(items);

            return count;
        }
    }
}
