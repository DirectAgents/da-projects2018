using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DACampSumLoader : Loader<CampaignSummary>
    {
        //private readonly DateTime date;
        private readonly Func<CampaignSummary, bool> KeepFunc;

        //public DACampSumLoader(DateTime date, bool keepAllNonZero = false)
        public DACampSumLoader(bool keepAllNonZero = false)
        {
            //this.date = date;
            if (keepAllNonZero)
                this.KeepFunc = cs => !cs.AllZeros();
            else
                this.KeepFunc = cs => (cs.Paid > 0 || cs.Revenue > 0 || cs.Cost > 0);

            LoadCurrencies();
        }

        private Dictionary<string, int> currencyIdLookupByAbbr = new Dictionary<string, int>();

        //private HashSet<int> offerIdsSaved = new HashSet<int>();
        //public IEnumerable<int> OfferIdsSaved
        //{
        //    get { return offerIdsSaved; }
        //}
        //private HashSet<int> campIdsSaved = new HashSet<int>();
        //public IEnumerable<int> CampIdsSaved
        //{
        //    get { return campIdsSaved; }
        //}
        //private Dictionary<int, DirectAgents.Domain.Entities.Cake.Offer> offerLookupById = new Dictionary<int, DirectAgents.Domain.Entities.Cake.Offer>();
        //private Dictionary<int, Camp> campLookupById = new Dictionary<int, Camp>();

        // TODO: hashset for aff ids

        protected override int Load(List<CampaignSummary> items)
        {
            Logger.Info("Loading {0} CampSums..", items.Count);
            AddMissingOffers(items);
            AddMissingAffiliates(items);
            AddMissingCampaigns(items);
            var count = UpsertCampSums(items);
            return count;
        }

        // used to determine which items to "keep"
        //private static Func<CampaignSummary, bool> KeepFunc =
        //    cs => (cs.Paid > 0 || cs.Revenue > 0 || cs.Cost > 0);

        private int UpsertCampSums(List<CampaignSummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            var deleted = 0;
            var alreadyDeleted = 0;
            using (var db = new DAContext())
            {
                bool toDelete;
                foreach (var item in items)
                {
                    toDelete = !KeepFunc(item);

                    var pk1 = item.Campaign.CampaignId;
                    var pk2 = item.Date;
                    var target = db.CampSums.Find(pk1, pk2);

                    if (toDelete)
                    {
                        if (target == null)
                            alreadyDeleted++;
                        else
                        {
                            db.CampSums.Remove(target);
                            deleted++;
                        }
                    }
                    else //to add/update
                    {
                        if (target == null)
                        {
                            target = new CampSum
                            {
                                CampId = item.Campaign.CampaignId,
                                Date = item.Date
                            };
                            item.CopyValuesTo(target);
                            db.CampSums.Add(target);
                            added++;
                        }
                        else
                        {   // update:
                            item.CopyValuesTo(target);
                            updated++;
                        }
                        //offerIdsSaved.Add(target.OfferId);
                        //campIdsSaved.Add(target.CampId);

                        //Update currencies...
                        var camp = db.Camps.Find(item.Campaign.CampaignId);
                        if (camp != null)
                        {
                            //CostCurr
                            if (camp.CurrencyAbbr == CurrencyAbbr.USD)
                                target.CostCurrId = CurrencyId.USD;
                            else
                            { // other than USD
                                // find currency match from currency table
                                if (currencyIdLookupByAbbr.ContainsKey(camp.CurrencyAbbr))
                                {
                                    target.CostCurrId = currencyIdLookupByAbbr[camp.CurrencyAbbr];
                                    target.Cost = camp.PayoutAmount * item.Paid;
                                    // recompute cost in foreign currency
                                }
                            }
                            var offerContract = db.OfferContracts.Find(camp.OfferContractId);
                            if (offerContract != null)
                            {
                                var offer = db.Offers.Find(offerContract.OfferId);
                                if (offer != null)
                                {
                                    //RevCurr
                                    if (offer.CurrencyAbbr == CurrencyAbbr.USD)
                                        target.RevCurrId = CurrencyId.USD;
                                    else
                                    { // other than USD
                                        // find currency match from currency table
                                        if (currencyIdLookupByAbbr.ContainsKey(offer.CurrencyAbbr))
                                        {
                                            target.RevCurrId = currencyIdLookupByAbbr[offer.CurrencyAbbr];
                                            target.Revenue = offerContract.ReceivedAmount * item.Paid;
                                            // recompute revenue in foreign currency
                                        }
                                    }
                                }
                            }
                        }
                    }
                    loaded++;
                }
                Logger.Info("Loading {0} CampSums ({1} updates, {2} additions, {3} deletions, {4} already-deleted)", loaded, updated, added, deleted, alreadyDeleted);
                db.SaveChanges();
            }
            return loaded;
        }

        private void LoadCurrencies()
        {
            using (var db = new DAContext())
            {
                currencyIdLookupByAbbr = db.Currencies.ToDictionary(c => c.Abbr, c => c.Id);
            }
        }

        private void AddMissingOffers(List<CampaignSummary> items)
        {
            int[] existingOfferIds;
            using (var db = new DAContext())
            {
                existingOfferIds = db.Offers.Select(x => x.OfferId).ToArray();
            }
            var neededOfferIds = items.Where(KeepFunc).Select(cs => cs.SiteOffer.SiteOfferId).Distinct();
            var missingOfferIds = neededOfferIds.Where(id => !existingOfferIds.Contains(id));

            //NOTE: this _should_ be okay since the CampSum extracter just makes one call to Cake, so that's done by now
            QuickETL_Offers(missingOfferIds);
        }
        public static void QuickETL_Offers(IEnumerable<int> offerIds)
        {
            if (!offerIds.Any())
                return;
            var extracter = new OffersExtracter(offerIds: offerIds);
            var loader = new DAOffersLoader(loadInactive: true);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

        private void AddMissingAffiliates(List<CampaignSummary> items)
        {
            int[] existingAffIds;
            using (var db = new DAContext())
            {
                existingAffIds = db.Affiliates.Select(x => x.AffiliateId).ToArray();
            }
            var neededAffIds = items.Where(KeepFunc).Select(cs => cs.SourceAffiliate.SourceAffiliateId).Distinct();
            var missingAffIds = neededAffIds.Where(id => !existingAffIds.Contains(id));

            // ?Could just use the SourceAffiliateId and SourceAffiliateName?
            // ?future: may be interested in other attributes?
            QuickETL_Affiliates(missingAffIds);
        }
        public static void QuickETL_Affiliates(IEnumerable<int> affIds)
        {
            if (!affIds.Any())
                return;
            var extracter = new AffiliatesExtracter(affiliateIds: affIds);
            var loader = new DAAffiliatesLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

        //TODO: make camp.AffId a foreign key
        private void AddMissingCampaigns(List<CampaignSummary> items)
        {
            int[] existingCampIds;
            using (var db = new DAContext())
            {
                existingCampIds = db.Camps.Select(x => x.CampaignId).ToArray();
            }
            var neededCampIds = items.Where(KeepFunc).Select(cs => cs.Campaign.CampaignId).Distinct();
            var missingCampIds = neededCampIds.Where(id => !existingCampIds.Contains(id));

            QuickETL_Campaigns(missingCampIds);
        }
        public static void QuickETL_Campaigns(IEnumerable<int> campIds)
        {
            if (!campIds.Any())
                return;
            var extracter = new CampaignsExtracter(campaignIds: campIds);
            var loader = new DACampLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

    }
}
