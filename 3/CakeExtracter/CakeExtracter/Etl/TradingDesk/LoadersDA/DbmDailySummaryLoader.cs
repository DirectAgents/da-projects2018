using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class DbmDailySummaryLoader : Loader<DbmRowBase>
    {
        private TDDailySummaryLoader dailySummaryLoader;
        private Dictionary<int, int> accountIdLookupByIOid = new Dictionary<int, int>();

        public DbmDailySummaryLoader()
        {
            this.dailySummaryLoader = new TDDailySummaryLoader();
        }

        protected override int Load(List<DbmRowBase> items)
        {
            Logger.Info("Loading {0} DailySummaries..", items.Count);
            AddUpdateDependentAccounts(items);
            var sItems = items.Select(i => CreateDailySummary(i, accountIdLookupByIOid[i.InsertionOrderID])).ToList();
            var count = dailySummaryLoader.UpsertDailySummaries(sItems);
            return count;
        }

        public static DailySummary CreateDailySummary(DbmRowBase item, int accountId)
        {
            var sum = new DailySummary
            {
                AccountId = accountId,
                Date = item.Date,
                Impressions = int.Parse(item.Impressions),
                Clicks = int.Parse(item.Clicks),
                PostClickConv = (int)decimal.Parse(item.PostClickConversions),
                PostViewConv = (int)decimal.Parse(item.PostViewConversions),
                PostClickRev = decimal.Parse(((DbmRow)item).PostClickRevenue),
                PostViewRev = decimal.Parse(((DbmRow)item).PostViewRevenue),
                Cost = decimal.Parse(item.Revenue)
            };
            return sum;
        }

        public void AddUpdateDependentAccounts(List<DbmRowBase> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            AddUpdateAccounts(ioTuples, accountIdLookupByIOid);
        }
        public static void AddUpdateAccounts(IEnumerable<Tuple<int, string>> ioTuples, Dictionary<int, int> accountIdLookupByIOid)
        {
            using (var db = new ClientPortalProgContext())
            {
                var dbmPlatformId = Platform.GetId(db, Platform.Code_DBM);
                var dbmAccounts = db.ExtAccounts.Where(a => a.PlatformId == dbmPlatformId);
                //var dbmExternalIds = dbmAccounts.Select(a => a.ExternalId).ToList();

                foreach (var ioTuple in ioTuples)
                {
                    int insertionOrderID = ioTuple.Item1;
                    string insertionOrderName = ioTuple.Item2;
                    if (accountIdLookupByIOid.ContainsKey(insertionOrderID))
                        continue; // already encountered this insertion order

                    var accounts = dbmAccounts.Where(a => a.ExternalId == insertionOrderID.ToString());
                    if (!accounts.Any())
                    { // add
                        var newAccount = new ExtAccount
                        {
                            PlatformId = dbmPlatformId,
                            ExternalId = insertionOrderID.ToString(),
                            Name = insertionOrderName
                        };
                        db.ExtAccounts.Add(newAccount);
                        db.SaveChanges();
                        Logger.Info("Added new ExtAccount from InsertionOrder: {0} ({1})", insertionOrderName, insertionOrderID);
                        accountIdLookupByIOid[insertionOrderID] = newAccount.Id;
                    }
                    else
                    {
                        // note: we're not updating any account names here
                        var account = accounts.First(); // there should be only one, but...
                        accountIdLookupByIOid[insertionOrderID] = account.Id;
                    }
                }
            }
        }

    }
}
