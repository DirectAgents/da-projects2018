using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cake.Data.Wsdl.ExportService;
using Cake.Model.Staging;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Wiki;

namespace DirectAgents.Domain.Concrete
{
    public class AdminImpl : IAdmin
    {
        private WikiContext wikiDomain;

        public event LogEventHandler LogHandler;
        protected void Log(string messageFormat, params object[] formatArgs)
        {
            Log(TraceEventType.Information, messageFormat, formatArgs);
        }
        protected void LogWarning(string messageFormat, params object[] formatArgs)
        {
            Log(TraceEventType.Warning, messageFormat, formatArgs);
        }
        protected void Log(TraceEventType severity, string messageFormat, params object[] formatArgs)
        {
            if (LogHandler != null)
                LogHandler(this, severity, messageFormat, formatArgs);
        }

        public string Test()
        {
            string text;
            Log("some information");
            LogWarning("some warning");
            using (wikiDomain = new WikiContext())
            {
                text = wikiDomain.Campaigns.Count() + " campaigns";
            }
            return text;
        }

        public string Test2()
        {
            string text;
            using (var db = new DAContext())
            {
                //var oi = new OfferInfo()
                //{
                //    OfferId = 1563,
                //    Budget = 250
                //};
                //db.OfferInfos.Add(oi);
                //db.SaveChanges();

                text = db.Offers.Count() + " offers";
            }
            return text;
        }

        public void CreateDatabaseIfNotExists()
        {
            using (var context = new WikiContext())
            {
                if (!context.Database.Exists())
                    context.Database.Create();
            }
        }

        //public void ReCreateDatabase()
        //{
        //    using (var context = new WikiContext())
        //    {
        //        if (context.Database.Exists())
        //            context.Database.Delete();
        //        context.Database.Create();
        //    }
        //}

        public IEnumerable<Campaign> CampaignsNotInCake()
        {
            using (var cake = new Cake.Model.Staging.CakeStagingEntities())
            using (wikiDomain = new WikiContext())
            {
                var cakePids = cake.CakeOffers.Select(o => o.Offer_Id).ToList();
                var campaigns = wikiDomain.Campaigns.Where(c => !cakePids.Contains(c.Pid));
                return campaigns.ToList();
            }
        }

        public void LoadSummaries()
        {
            Log("start LoadSummaries");
            using (var cake = new Cake.Model.Staging.CakeStagingEntities())
            using (wikiDomain = new WikiContext())
            {
                var pids = wikiDomain.Campaigns.Select(c => c.Pid).ToList();
                foreach (var pid in pids)
                {
                    Log("Pid {0}", pid);
                    var cakeSummaries = cake.DailySummaries.Where(ds => ds.offer_id == pid);
                    if (!cakeSummaries.Any())
                    {
                        LogWarning("No cake summaries found; done");
                    }
                    else
                    {
                        var existingSummaries = wikiDomain.DailySummaries.Where(ds => ds.Pid == pid);
                        if (existingSummaries.Any())
                        {
                            // Try deleting today's and yesterday's summaries so they can be updated
                            var yesterday = DateTime.Today.AddDays(-1);
                            var recentSummaries = existingSummaries.Where(s => s.Date >= yesterday);
                            if (!recentSummaries.Any())
                            {   // Or... delete the most recent one if we have something to replace it with
                                // (if the updater stopped working for more than a day, the most recent one might not have complete stats)
                                var mostRecentExistingDate = existingSummaries.Max(es => es.Date);
                                if (cakeSummaries.Any(cs => cs.date == mostRecentExistingDate))
                                    recentSummaries = existingSummaries.Where(es => es.Date == mostRecentExistingDate);
                            }
                            if (recentSummaries.Any())
                            {
                                Log("Deleting most recent summaries");
                                foreach (var summary in recentSummaries)
                                {
                                    wikiDomain.DailySummaries.Remove(summary);
                                }
                                wikiDomain.SaveChanges();
                            }
                        }
                        // See if any existing summaries remain
                        if (!existingSummaries.Any())
                        {
                            Log("No existing summaries");
                        }
                        else
                        {
                            var maxDate = existingSummaries.Max(ds => ds.Date);
                            Log("Found existing summaries through {0}", maxDate);
                            cakeSummaries = cakeSummaries.Where(ds => ds.date > maxDate);
                        }
                        Log("Loading {0} new summaries", cakeSummaries.Count());
                        foreach (var cakeSummary in cakeSummaries)
                        {
                            var daSummary = new DirectAgents.Domain.Entities.Wiki.DailySummary
                            {
                                Pid = cakeSummary.offer_id,
                                Date = cakeSummary.date,
                                Clicks = cakeSummary.clicks,
                                Conversions = cakeSummary.conversions,
                                Paid = cakeSummary.paid,
                                Sellable = cakeSummary.sellable,
                                Cost = cakeSummary.cost,
                                Revenue = cakeSummary.revenue
                            };
                            wikiDomain.DailySummaries.Add(daSummary);
                        }
                        wikiDomain.SaveChanges();
                        Log("done");
                    }
                }
            }
            Log("done LoadSummaries");
        }

        public void LoadCampaigns()
        {
            Log("start LoadCampaigns");
            using (var cake = new Cake.Model.Staging.CakeStagingEntities())
            using (wikiDomain = new WikiContext())
            {
                Log("Updating verticals");
                UpdateVerticals(cake);
                Log("Updating traffic types");
                UpdateTrafficTypes(cake);

                var countryLookup = new CountryLookup(cake);

                Log("Going through cake staged campaigns...");
                var offerObjects = StagedOffers(cake);
                foreach (var item in offerObjects)
                {
                    var campaign = GetOrCreateCampaign(item);

                    campaign.Name = item.Offer.OfferName;
                    campaign.Vertical = wikiDomain.Verticals.Single(c => c.Name == item.Offer.VerticalName);

                    var offer = (Cake.Data.Wsdl.ExportService.offer1)item.Offer;

                    if (offer != null)
                    {
                        ExtractFieldsFromWsdlOffer(campaign, offer);
                        if (!wikiDomain.Statuses.Any(s => s.StatusId == offer.StatusId))
                        {
                            var status = new Status { StatusId = offer.StatusId, Name = offer.StatusName };
                            wikiDomain.Statuses.Add(status);
                        }
                    }
                    AddCountries(countryLookup[campaign.Pid], campaign);
                    AddTrafficTypes(item.Offer.AllowedMediaTypeNames.SplitCSV().Distinct(), campaign);
                    AddAccountManagers(item.Advertiser.AccountManagerName, campaign);
                    AddAdManagers(item.Advertiser.AdManagerName, campaign);
                    wikiDomain.SaveChanges();
                }
            }
            Log("done LoadCampaigns");
        }

        private static void ExtractFieldsFromWsdlOffer(Campaign campaign, offer1 offer)
        {
            campaign.ImageUrl = offer.offer_image_link;
            campaign.Description = string.IsNullOrWhiteSpace(offer.offer_description) ? "no description" : offer.offer_description;
            campaign.Link = offer.preview_link;
            campaign.Restrictions = offer.restrictions;
            campaign.Hidden = offer.hidden;
            campaign.StatusId = offer.StatusId;

            campaign.DefaultPriceFormat = offer.DefaultPriceFormat;

            offer_contract offer_contract = offer.offer_contracts.FirstOrDefault(oc => oc.offer_contract_id == offer.default_offer_contract_id);
            if (offer_contract != null)
            {
                campaign.RevenueIsPercentage = offer_contract.received.is_percentage;
                campaign.Revenue = offer_contract.received.amount;

                if (offer.DefaultPriceFormat == "RevShare")
                    campaign.Cost = offer_contract.payout.amount;
                else
                {   // compute cost as 2/3 of revenue
                    if (campaign.Revenue < 1.5m) // computed cost will be less than 1.00
                        campaign.Cost = decimal.Round(campaign.Revenue * (40m / 3m), 0) / 20; // round to nearest 0.05
                    //campaign.Cost = decimal.Round(campaign.Revenue * (2m / 3m), 2); // round to nearest 0.01
                    else
                        campaign.Cost = decimal.Round(campaign.Revenue * (8m / 3m), 0) / 4; // round to nearest 0.25
                }
            }
            campaign.CostCurrency = offer.currency.currency_symbol;
            campaign.RevenueCurrency = offer.currency.currency_symbol;
        }

        private void UpdateVerticals(CakeStagingEntities cake)
        {
            var verticals = wikiDomain.Verticals.ToList();
            var currentVerticals = verticals.Select(c => c.Name);

            var usedVerticalIds = wikiDomain.Campaigns.Select(c => c.Vertical.VerticalId).Distinct();
            var currentUnusedVerticals = wikiDomain.Verticals.Where(v => !usedVerticalIds.Contains(v.VerticalId)).Select(v => v.Name).ToList();

            var stagedVerticals = cake.CakeOffers.Select(o => o.VerticalName).Distinct();

            foreach (var item in currentUnusedVerticals.Except(stagedVerticals).Select(vn => verticals.Single(v => v.Name == vn)))
                wikiDomain.Verticals.Remove(item);

            foreach (var item in stagedVerticals.Except(currentVerticals))
                wikiDomain.Verticals.Add(new Vertical { Name = item });

            wikiDomain.SaveChanges();
        }

        private void UpdateTrafficTypes(CakeStagingEntities cake)
        {
            var trafficTypes = wikiDomain.TrafficTypes.ToList();
            var staged = cake.CakeOffers.ToList().SelectMany(c => c.AllowedMediaTypeNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).Distinct();
            var current = trafficTypes.Select(c => c.Name);

            foreach (var item in current.Except(staged).Select(c => trafficTypes.Single(d => d.Name == c)))
                wikiDomain.TrafficTypes.Remove(item);

            foreach (var item in staged.Except(current))
                wikiDomain.TrafficTypes.Add(new TrafficType { Name = item });

            wikiDomain.SaveChanges();
        }

        private class _OfferAndAdvertiser
        {
            public CakeOffer Offer { get; set; }
            public CakeAdvertiser Advertiser { get; set; }
        }

        private static IEnumerable<_OfferAndAdvertiser> StagedOffers(CakeStagingEntities cake)
        {
            var query = from offer in cake.CakeOffers.ToList()
                        from advertiser in cake.CakeAdvertisers.ToList()
                        where advertiser.Advertiser_Id == int.Parse(offer.Advertiser_Id)
                        select new _OfferAndAdvertiser { Offer = offer, Advertiser = advertiser };

            return query;
        }

        Campaign GetOrCreateCampaign(_OfferAndAdvertiser item)
        {
            var campaign = wikiDomain.Campaigns.FirstOrDefault(c => c.Pid == item.Offer.Offer_Id);
            if (campaign == null)
            {
                campaign = new Campaign { Pid = item.Offer.Offer_Id, };
                wikiDomain.Campaigns.Add(campaign);
                Log("Pid {0} (adding)", campaign.Pid);
            }
            else
            {
                Log("Pid {0} (updating)", campaign.Pid);
            }

            return campaign;
        }

        // TODO: support multiple accountmangers
        private void AddAccountManagers(string accountManagerName, Campaign campaign)
        {
            var am = wikiDomain.People.Where(p => p.Name == accountManagerName).FirstOrDefault();

            if (am == null)
                am = new Person { Name = accountManagerName };

            campaign.AccountManagers.Clear();
            campaign.AccountManagers.Add(am);
        }

        // TODO: support multiple admangers
        private void AddAdManagers(string adManagerName, Campaign campaign)
        {
            var ad = wikiDomain.People.Where(p => p.Name == adManagerName).FirstOrDefault();

            if (ad == null)
                ad = new Person { Name = adManagerName };

            if (campaign.AdManagers != null)
            {
                campaign.AdManagers.Clear();
                campaign.AdManagers.Add(ad);
            }
        }

        private void AddCountries(IEnumerable<string> countryCodes, Campaign campaign)
        {
            campaign.Countries.Clear();

            foreach (var countryCode in countryCodes)
            {
                var country = wikiDomain.Countries.Where(c => c.CountryCode == countryCode).FirstOrDefault();

                if (country == null)
                    country = new Country { CountryCode = countryCode, Name = countryCode + "." };

                campaign.Countries.Add(country);
            }
        }

        private void AddTrafficTypes(IEnumerable<string> trafficTypeNames, Campaign campaign)
        {
            campaign.TrafficTypes.Clear();

            foreach (var trafficTypeName in trafficTypeNames)
            {
                var trafficType = wikiDomain.TrafficTypes.Where(c => c.Name == trafficTypeName).FirstOrDefault();

                if (trafficType == null)
                    trafficType = new TrafficType { Name = trafficTypeName };

                campaign.TrafficTypes.Add(trafficType);
            }
        }
    }
}
