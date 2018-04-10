using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class DbmLineItemSummaryLoader : Loader<DbmRowBase>
    {
        private TDStrategySummaryLoader strategySummaryLoader;
        private Dictionary<int, int> accountIdLookupByIOid = new Dictionary<int, int>();
        private Dictionary<string, int> strategyIdLookupByLineItemId = new Dictionary<string, int>();

        public DbmLineItemSummaryLoader()
        {
            this.strategySummaryLoader = new TDStrategySummaryLoader();
        }

        protected override int Load(List<DbmRowBase> items)
        {
            Logger.Info("Loading {0} LineItem DailySummaries..", items.Count);
            AddUpdateDependentAccounts(items);
            AddUpdateDependentStrategies(items);
            var ssItems = items.Select(i => CreateStrategySummary(i, strategyIdLookupByLineItemId[((DbmRowWithLineItem)i).LineItemID])).ToList();
            var count = strategySummaryLoader.UpsertDailySummaries(ssItems);
            return count;
        }

        public static StrategySummary CreateStrategySummary(DbmRowBase item, int strategyId)
        {
            var sSum = new StrategySummary
            {
                StrategyId = strategyId,
                Date = item.Date,
                Impressions = int.Parse(item.Impressions),
                Clicks = int.Parse(item.Clicks),
                PostClickConv = (int)decimal.Parse(item.PostClickConversions),
                PostViewConv = (int)decimal.Parse(item.PostViewConversions),
                PostClickRev = decimal.Parse(((DbmRowWithLineItem)item).PostClickRevenue),
                PostViewRev = decimal.Parse(((DbmRowWithLineItem)item).PostViewRevenue),
                Cost = decimal.Parse(item.Revenue)
            };
            return sSum;
        }

        public void AddUpdateDependentAccounts(List<DbmRowBase> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            DbmDailySummaryLoader.AddUpdateAccounts(ioTuples, accountIdLookupByIOid);
        }

        private void AddUpdateDependentStrategies(List<DbmRowBase> items)
        {
            var tuples = items.Select(i => Tuple.Create(((DbmRowWithLineItem)i).LineItemID, ((DbmRowWithLineItem)i).LineItem, i.InsertionOrderID)).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var tuple in tuples)
                {
                    string lineItemID = tuple.Item1;
                    string lineItemName = tuple.Item2;
                    int ioID = tuple.Item3;

                    if (strategyIdLookupByLineItemId.ContainsKey(lineItemID))
                        continue; // already encountered this strategy
                    if (!accountIdLookupByIOid.ContainsKey(ioID))
                        continue; // should have been added in AddUpdateDependentAccounts()
                    int accountId = accountIdLookupByIOid[ioID];

                    var stratsInDb = db.Strategies.Where(s => s.AccountId == accountId && s.ExternalId == lineItemID);
                    if (!stratsInDb.Any())
                    {   // Strategy doesn't exist in the db; so create it and put an entry in the lookup
                        var strategy = new Strategy
                        {
                            AccountId = accountId,
                            ExternalId = lineItemID,
                            Name = lineItemName
                            // other properties...
                        };
                        db.Strategies.Add(strategy);
                        db.SaveChanges();
                        Logger.Info("Saved new Strategy: {0} ({1}), ExternalId={2}", strategy.Name, strategy.Id, strategy.ExternalId);
                        strategyIdLookupByLineItemId[lineItemID] = strategy.Id;
                    }
                    else
                    {   // Update & put existing Strategy in the lookup
                        // There should only be one matching Strategy in the db, but just in case...
                        foreach (var strat in stratsInDb)
                        {
                            if (!string.IsNullOrWhiteSpace(lineItemName))
                                strat.Name = lineItemName;
                            // other properties...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated Strategy: {0}, Eid={1}", lineItemName, lineItemID);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        strategyIdLookupByLineItemId[lineItemID] = stratsInDb.First().Id;
                    }
                }
            }
        }

    }
}
