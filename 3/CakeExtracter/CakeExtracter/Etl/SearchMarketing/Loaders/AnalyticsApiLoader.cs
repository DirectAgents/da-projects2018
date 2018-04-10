using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Etl.SearchMarketing.Loaders
{
    public class AnalyticsApiLoader : Loader<AnalyticsRow>
    {
        private readonly int advertiserId;

        public AnalyticsApiLoader(int advertiserId)
        {
            this.advertiserId = advertiserId;
        }

        protected override int Load(List<AnalyticsRow> rows)
        {
            Logger.Info("Loading {0} Analytics rows..", rows.Count);
            AddDependentSearchCampaigns(rows);
            var count = UpsertAnalyticsRows(rows);
            return count;
        }

        private int UpsertAnalyticsRows(List<AnalyticsRow> rows)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalContext())
            {
                foreach (var row in rows)
                {
                    SearchCampaign campaign = null;
                    if (row.CampaignId.HasValue)
                        campaign = db.SearchCampaigns.SingleOrDefault(c => c.ExternalId == row.CampaignId && c.AdvertiserId == this.advertiserId);

                    if (campaign == null)
                    {   // try matching by campaign name
                        var channel = row.CampaignName.ToLower().StartsWith("bing") ? "bing" : row.Source;

                        var campaigns = db.SearchCampaigns.Where(c => c.AdvertiserId == this.advertiserId && c.Channel == channel).ToList();
                        campaigns = campaigns.Where(c => c.SearchCampaignName.Replace(" ", "").Contains(row.CampaignName.Replace(" ", ""))).ToList();
                        if (campaigns.Count > 0)
                            campaign = campaigns[0]; // TODO: if more than one, throw exception?
                        else
                            Logger.Info("AnalyticsApiLoader.Upsert... could not find a matching campaign for: {0}", row.CampaignName);
                    }
                    if (campaign != null)
                    {
                        var pk1 = campaign.SearchCampaignId;
                        var pk2 = row.Date;
                        var source = new GoogleAnalyticsSummary
                        {
                            SearchCampaignId = pk1,
                            Date = pk2,
                            Transactions = row.Transactions,
                            Revenue = row.Revenue
                        };

                        var target = db.Set<GoogleAnalyticsSummary>().Find(pk1, pk2);
                        if (target == null)
                        {
                            db.GoogleAnalyticsSummaries.Add(source);
                            addedCount++;
                        }
                        else
                        {
                            db.Entry(target).State = EntityState.Detached;
                            target = AutoMapper.Mapper.Map(source, target);
                            db.Entry(target).State = EntityState.Modified;
                            updatedCount++;
                        }
                        itemCount++;

                        //TODO: switch back to using SearchDailySummary ?

                        // See if there's a search daily summary row for the same campaign & date...
                        if (!db.Set<SearchDailySummary2>().Any(s => s.SearchCampaignId == pk1 && s.Date == pk2))
                        {   // ...if not, add one, so that left-joining to it can happen later (when querying stats)
                            Logger.Info("Creating empty search daily summary record for campaign {0} for {1}", pk1, pk2.ToShortDateString());
                            var sds = new SearchDailySummary2()
                            {
                                SearchCampaignId = pk1,
                                Date = pk2,
                                Network = ".",
                                Device = ".",
                                ClickType = ".",
                                Revenue = 0,
                                Cost = 0,
                                Orders = 0,
                                Clicks = 0,
                                Impressions = 0,
                                CurrencyId = 1 // item["CurrencyCode"] == "USD" ? 1 : -1 // NOTE: non USD (if exists) -1 for now
                            };
                            db.SearchDailySummary2.Add(sds);
                        }
                    }
                }
                Logger.Info("Saving {0} GoogleAnalyticsSummaries ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        private void AddDependentSearchCampaigns(List<AnalyticsRow> rows)
        {
            using (var db = new ClientPortalContext())
            {
                foreach (var tuple in rows.Select(r => Tuple.Create(r.CampaignId, r.CampaignName, r.Source)).Distinct())
                {
                    var campaignId = tuple.Item1;
                    var campaignName = tuple.Item2;

                    var channel = campaignName.ToLower().StartsWith("bing") ? "bing" : tuple.Item3;

                    SearchCampaign existing = null;

                    if (campaignId.HasValue)
                    {
                        existing = db.SearchCampaigns.SingleOrDefault(c => c.ExternalId == campaignId && c.AdvertiserId == this.advertiserId && c.Channel == channel);
                    }
                    else // no (adwords) campaignId... try to match on the campaign name (removing spaces)
                    {
                        var campaigns = db.SearchCampaigns.Where(c => c.AdvertiserId == this.advertiserId && c.Channel == channel).ToList();
                        campaigns = campaigns.Where(c => c.SearchCampaignName.Replace(" ", "").Contains(campaignName.Replace(" ", ""))).ToList();
                        if (campaigns.Count > 0) // TODO: what to do if more than one?
                        {
                            existing = campaigns[0];
                            campaignName = existing.SearchCampaignName;
                        }
                        else
                            Logger.Info("AnalyticsApiLoader.AddDependentSearchCampaigns- could not find a matching campaign for: {0}", campaignName);
                    }

                    if (existing == null)
                    {
                        db.SearchCampaigns.Add(new SearchCampaign
                        {
                            AdvertiserId = this.advertiserId,
                            SearchCampaignName = campaignName,
                            Channel = channel,
                            ExternalId = campaignId
                        });
                        Logger.Info("Saving new SearchCampaign: {0} [{1}] (channel {2}, advertiser {3})", campaignName, campaignId, channel, this.advertiserId);
                        db.SaveChanges();
                    }
                    else if (existing.SearchCampaignName != campaignName)
                    {
                        existing.SearchCampaignName = campaignName;
                        Logger.Info("Saving updated SearchCampaign name: {0} [{1}] (channel {2}, advertiser {3})", campaignName, campaignId, channel, this.advertiserId);
                        db.SaveChanges();
                    }
                }
            }
        }

    }
}
