using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClientPortal.Data.Contexts;
using Criteo.CriteoAPI;

namespace CakeExtracter.Etl.SearchMarketing.Loaders
{
    public class CriteoDailySummaryLoader : Loader<Dictionary<string, string>>
    {
        private const string criteoChannel = "Criteo";
        private readonly int searchAccountId;

        public CriteoDailySummaryLoader(int searchAccountId)
        {
            this.searchAccountId = searchAccountId;
        }

        public void AddUpdateSearchCampaigns(campaign[] campaigns)
        {
            AddUpdateSearchCampaigns(campaigns, this.searchAccountId);
        }
        public static void AddUpdateSearchCampaigns(campaign[] campaigns, int searchAccountId)
        {
            using (var db = new ClientPortalContext())
            {
                var searchAccount = db.SearchAccounts.Find(searchAccountId);

                foreach (var campaign in campaigns)
                {
                    var searchCampaign = db.SearchCampaigns.SingleOrDefault(sc => sc.ExternalId == campaign.campaignID);
                    if (searchCampaign == null)
                    {
                        searchAccount.SearchCampaigns.Add(new SearchCampaign
                        {
                            SearchCampaignName = campaign.campaignName,
                            ExternalId = campaign.campaignID
                        });
                        Logger.Info("Saving new SearchCampaign: {0} ({1})", campaign.campaignName, campaign.campaignID);
                        db.SaveChanges();
                    }
                    else if (searchCampaign.SearchCampaignName != campaign.campaignName)
                    {
                        searchCampaign.SearchCampaignName = campaign.campaignName;
                        Logger.Info("Saving updated SearchCampaign name: {0} ({1})", campaign.campaignName, campaign.campaignID);
                        db.SaveChanges();
                    }
                }
            }
        }

        protected override int Load(List<Dictionary<string, string>> items)
        {
            Logger.Info("Loading {0} SearchDailySummaries..", items.Count);
            //AddUpdateDependentSearchCampaigns(items);
            var count = UpsertSearchDailySummaries(items);
            return count;
        }

        private int UpsertSearchDailySummaries(List<Dictionary<string, string>> items)
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
                    var campaignID = int.Parse(item["campaignID"]);
                    var searchCampaign = searchAccount.SearchCampaigns.SingleOrDefault(c => c.ExternalId == campaignID);
                    if (searchCampaign != null)
                    {
                        var pk1 = searchCampaign.SearchCampaignId;
                        var pk2 = DateTime.Parse(item["dateTime"]);
                        var pk3 = ".";
                        var pk4 = ".";
                        var source = new SearchDailySummary
                        {
                            SearchCampaignId = pk1,
                            Date = pk2,
                            Network = pk3,
                            Device = pk4,
                            Revenue = decimal.Parse(item["orderValue"]),
                            Cost = decimal.Parse(item["cost"]),
                            Orders = int.Parse(item["sales"]),
                            Clicks = int.Parse(item["click"]),
                            Impressions = int.Parse(item["impressions"]),
                            CurrencyId = 1 // item["CurrencyCode"] == "USD" ? 1 : -1 // NOTE: non USD (if exists) -1 for now
                        };
                        var target = db.Set<SearchDailySummary>().Find(pk1, pk2, pk3, pk4);
                        if (target == null)
                        {
                            db.SearchDailySummaries.Add(source);
                            addedCount++;
                        }
                        else
                        {
                            var entry = db.Entry(target);
                            entry.State = EntityState.Detached;
                            AutoMapper.Mapper.Map(source, target);
                            entry.State = EntityState.Modified;
                            updatedCount++;
                        }
                    }
                    else
                    {
                        Logger.Warn("SearchCampaign {0} not found (SearchAccount {1}); skipping load of item", campaignID, searchAccountId);
                        skippedCount++;
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
