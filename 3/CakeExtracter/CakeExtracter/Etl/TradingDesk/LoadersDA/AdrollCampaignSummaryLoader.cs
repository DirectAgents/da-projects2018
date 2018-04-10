using System.Collections.Generic;
using System.Linq;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollCampaignSummaryLoader : Loader<CampaignSummary>
    {
        private readonly int accountId;
        private TDStrategySummaryLoader strategySummaryLoader;
        private Dictionary<string, int> strategyIdLookupByCampEid = new Dictionary<string, int>();

        public AdrollCampaignSummaryLoader(string advertisableEid)
        {
            using (var db = new ClientPortalProgContext())
            {
                var account = db.ExtAccounts.Where(a => a.ExternalId == advertisableEid && a.Platform.Code == Platform.Code_AdRoll)
                                            .FirstOrDefault();
                if (account != null)
                    accountId = account.Id;
                else
                    accountId = -1;

                this.strategySummaryLoader = new TDStrategySummaryLoader(accountId);
            }
        }
        public bool FoundAccount()
        {
            return (accountId > -1);
        }

        protected override int Load(List<CampaignSummary> items)
        {
            Logger.Info("Loading {0} Adroll CampaignDailySummaries..", items.Count);
            AddUpdateDependentStrategies(items);
            var ssItems = items.Select(cSum => CreateStrategySummary(cSum, strategyIdLookupByCampEid[cSum.eid])).ToList();
            var count = strategySummaryLoader.UpsertDailySummaries(ssItems);
            return count;
        }

        public static StrategySummary CreateStrategySummary(CampaignSummary cSum, int strategyId)
        {
            var sSum = new StrategySummary
            {
                StrategyId = strategyId,
                Date = cSum.date,
                Impressions = cSum.impressions,
                Clicks = cSum.clicks,
                PostClickConv = cSum.click_through_conversions,
                PostViewConv = cSum.view_through_conversions,
                PostClickRev = (decimal)cSum.adjusted_attributed_click_through_rev,
                PostViewRev = (decimal)cSum.adjusted_attributed_view_through_rev,
                Cost = (decimal)cSum.cost
                // Prospects?
            };
            return sSum;
        }

        private void AddUpdateDependentStrategies(List<CampaignSummary> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                // Find the unique campaigns by grouping
                var itemGroups = items.GroupBy(i => new { i.eid, i.campaign });
                foreach (var group in itemGroups)
                {
                    if (strategyIdLookupByCampEid.ContainsKey(group.Key.eid))
                        continue; // already encountered this eid

                    // See if a Strategy with that ExternalId exists
                    var stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.ExternalId == group.Key.eid);

                    //// If not, check by name
                    //if (stratsInDb.Count() == 0)
                    //    stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s => s.Name == group.Key.campaign);

                    // Assume all strategies in the group have the same properties (just different dates/stats)
                    //var groupStrat = group.First();

                    if (!stratsInDb.Any())
                    {   // Strategy doesn't exist in the db; so create it and put an entry in the lookup
                        var strategy = new Strategy
                        {
                            AccountId = this.accountId,
                            ExternalId = group.Key.eid,
                            Name = group.Key.campaign
                            // type, status, created, start, end, etc...
                        };
                        db.Strategies.Add(strategy);
                        db.SaveChanges();
                        Logger.Info("Saved new Strategy: {0} ({1}), ExternalId={2}", strategy.Name, strategy.Id, strategy.ExternalId);
                        strategyIdLookupByCampEid[strategy.ExternalId] = strategy.Id;
                    }
                    else
                    {   // Update & put existing Strategy in the lookup
                        // There should only be one matching Strategy in the db, but just in case...
                        foreach (var strat in stratsInDb)
                        {
                            strat.ExternalId = group.Key.eid;
                            strat.Name = group.Key.campaign;
                            // type, status, created, start, end, etc...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated Strategy: {0}, Eid={1}", group.Key.campaign, group.Key.eid);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        strategyIdLookupByCampEid[group.Key.eid] = stratsInDb.First().Id;
                    }
                }
            }
        }
    }
}
