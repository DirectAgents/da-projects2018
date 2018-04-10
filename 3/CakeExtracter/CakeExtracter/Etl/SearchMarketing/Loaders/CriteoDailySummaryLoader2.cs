using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClientPortal.Data.Contexts;
using Criteo.CriteoAPI;

namespace CakeExtracter.Etl.SearchMarketing.Loaders
{
    public class CriteoDailySummaryLoader2 : Loader<SearchDailySummary>
    {
        private const string criteoChannel = "Criteo";
        private readonly int searchAccountId;

        public CriteoDailySummaryLoader2(int searchAccountId)
        {
            this.searchAccountId = searchAccountId;
        }

        public void AddUpdateSearchCampaigns(campaign[] campaigns)
        {
            CriteoDailySummaryLoader.AddUpdateSearchCampaigns(campaigns, this.searchAccountId);
        }

        protected override int Load(List<SearchDailySummary> items)
        {
            Logger.Info("Loading {0} SearchDailySummaries..", items.Count);
            //AddUpdateDependentSearchCampaigns(items);
            var count = UpsertSearchDailySummaries(items);
            return count;
        }

        private int UpsertSearchDailySummaries(List<SearchDailySummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalContext())
            {
                var searchAccount = db.SearchAccounts.Find(this.searchAccountId);

                foreach (var item in items)
                {
                    var searchCampaign = searchAccount.SearchCampaigns.SingleOrDefault(c => c.ExternalId == item.SearchCampaignId);
                    if (searchCampaign != null)
                    {
                        item.SearchCampaignId = searchCampaign.SearchCampaignId; // replace what was the external id
                        item.Network = ".";
                        item.Device = ".";
                        item.CurrencyId = 1; // item["CurrencyCode"] == "USD" ? 1 : -1 // NOTE: non USD (if exists) -1 for now

                        var target = db.Set<SearchDailySummary>().Find(item.SearchCampaignId, item.Date, item.Network, item.Device);
                        if (target == null)
                        {
                            db.SearchDailySummaries.Add(item);
                            addedCount++;
                        }
                        else
                        {
                            var entry = db.Entry(target);
                            entry.State = EntityState.Detached;
                            AutoMapper.Mapper.Map(item, target);
                            entry.State = EntityState.Modified;
                            updatedCount++;
                        }
                    }
                    else
                    {
                        Logger.Warn("SearchCampaign {0} not found (SearchAccount {1}); skipping load of item", item.SearchCampaignId, searchAccountId);
                        skippedCount++;          // (external id)
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} SearchDailySummaries ({1} updates, {2} additions, {3} skipped)", itemCount, updatedCount, addedCount, skippedCount);
                db.SaveChanges();
            }
            return itemCount;
        }
    }
}
