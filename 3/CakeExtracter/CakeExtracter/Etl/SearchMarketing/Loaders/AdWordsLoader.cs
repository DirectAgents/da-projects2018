using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Etl.SearchMarketing.Loaders
{
    // TODO: remove this class all together and then make AdWordsApiLoader accept an xml file without 
    //       calling the Api
    public class AdWordsLoader : Loader<Dictionary<string, string>>
    {
        protected override int Load(List<Dictionary<string, string>> items)
        {
            Logger.Info("Loading {0} SearchDailySummaries..", items.Count);
            AddDependentAdvertisers(items);
            AddDependentSearchCampaigns(items);
            var count = UpsertSearchDailySummaries(items);
            return count;
        }

        private static int UpsertSearchDailySummaries(List<Dictionary<string, string>> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalContext())
            {
                foreach (var item in items)
                {
                    var advertiserName = item["account"];
                    var advertiserId = db.Advertisers.Single(c => c.AdvertiserName == advertiserName).AdvertiserId;
                    var campaignName = item["campaign"];
                    var pk1 = db.SearchCampaigns.Single(c => c.SearchCampaignName == campaignName && c.AdvertiserId == advertiserId && c.Channel == "google").SearchCampaignId;
                    var pk2 = DateTime.Parse(item["day"].Replace('-', '/'));
                    var pk3 = item["campaignType"].Substring(0, 1);
                    var pk4 = item["device"].Substring(0, 1);
                    var pk5 = item["clickType"].Substring(0, 1);
                    var source = new SearchDailySummary2
                    {
                        SearchCampaignId = pk1,
                        Date = pk2,
                        Network = pk3,
                        Device = pk4,
                        ClickType = pk5,
                        Revenue = decimal.Parse(item["totalConvValue"]),
                        Cost = decimal.Parse(item["cost"]),
                        //Orders = int.Parse(item["conv1PerClick"]),
                        Orders = int.Parse(item["convertedClicks"]),
                        Clicks = int.Parse(item["clicks"]),
                        Impressions = int.Parse(item["impressions"]),
                        CurrencyId = (!item.Keys.Contains("currency") || item["currency"] == "USD") ? 1 : -1 // NOTE: non USD (if exists) -1 for now
                    };
                    var target = db.Set<SearchDailySummary2>().Find(pk1, pk2, pk3, pk4, pk5);
                    if (target == null)
                    {
                        db.SearchDailySummary2.Add(source);
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
                    itemCount++;
                }
                Logger.Info("Saving {0} SearchDailySummaries ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        private void AddDependentAdvertisers(List<Dictionary<string, string>> items)
        {
            using (var db = new ClientPortalContext())
            {
                foreach (var advertiserName in items.Select(c => c["account"]).Distinct())
                {
                    if (!db.Advertisers.Any(c => c.AdvertiserName == advertiserName))
                    {
                        // Get next advertiser in range of low and high number
                        const int lowAdvertiserId = 90000;
                        const int highAdvertiserId = 100000;
                        int advertiserId;
                        var advertiserIdQuery = db.Advertisers.Select(c => c.AdvertiserId)
                                                  .Where(c => c >= lowAdvertiserId && c <= highAdvertiserId);
                        if (advertiserIdQuery.Any())
                        {
                            advertiserId = advertiserIdQuery.Max() + 1;
                            if (advertiserId > highAdvertiserId)
                                throw new Exception("too many advertisers");
                        }
                        else
                        {
                            advertiserId = lowAdvertiserId;
                        }
                        db.Advertisers.Add(new Advertiser
                            {
                                AdvertiserId = advertiserId,
                                AdvertiserName = advertiserName,
                                Culture = "en-US",
                                HasSearch = true
                            });
                        Logger.Info("Saving new Advertiser: {0} ({1})", advertiserName, advertiserId);
                        db.SaveChanges();
                    }
                }
            }
        }

        private void AddDependentSearchCampaigns(List<Dictionary<string, string>> items)
        {
            using (var db = new ClientPortalContext())
            {
                foreach (var tuple in items.Select(c => Tuple.Create(c["account"], c["campaign"])).Distinct())
                {
                    var advertiserName = tuple.Item1;
                    var advertiser = db.Advertisers.Single(c => c.AdvertiserName == advertiserName);
                    var campaignName = tuple.Item2;
                    if (!db.SearchCampaigns.Any(c => c.SearchCampaignName == campaignName && c.AdvertiserId == advertiser.AdvertiserId && c.Channel == "google"))
                    {
                        db.SearchCampaigns.Add(new SearchCampaign
                        {
                            Advertiser = advertiser,
                            SearchCampaignName = campaignName,
                            Channel = "google"
                        });
                        Logger.Info("Saving new SearchCampaign: {0} ({1})", campaignName, advertiserName);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
