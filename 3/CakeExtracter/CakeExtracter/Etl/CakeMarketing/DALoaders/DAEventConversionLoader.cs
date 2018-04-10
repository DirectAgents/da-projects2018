using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using DirectAgents.Domain.Contexts;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAEventConversionLoader : Loader<EventConversion>
    {

        protected override int Load(List<EventConversion> items)
        {
            Logger.Info("Loading {0} EventConversions..", items.Count);
            AddMissingOffers(items);
            AddMissingAffiliates(items);
            AddUpdateDependentEvents(items);
            AddUpdateDependentPriceFormats(items);
            var count = UpsertEventConversions(items);
            return count;
        }

        private int UpsertEventConversions(List<EventConversion> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var itemCount = 0;
            using (var db = new DAContext())
            {
                foreach (var item in items)
                {
                    var pk1 = item.EventConversionId;
                    var target = db.EventConversions.Find(pk1);
                    if (target == null)
                    {
                        var ec = new DirectAgents.Domain.Entities.Cake.EventConversion
                        {
                            Id = item.EventConversionId
                        };
                        item.CopyValuesTo(ec);
                        db.EventConversions.Add(ec);
                        addedCount++;
                    }
                    else
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            item.CopyValuesTo(target);
                            updatedCount++;
                        }
                        else
                        {
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} EventConversions ({1} updates, {2} additions, {3} duplicates)",
                            itemCount, updatedCount, addedCount, duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        public static void AddMissingOffers(List<EventConversion> items)
        {
            int[] existingOfferIds;
            using (var db = new DAContext())
            {
                existingOfferIds = db.Offers.Select(x => x.OfferId).ToArray();
            }
            var neededOfferIds = items.Select(cs => cs.SiteOffer.SiteOfferId).Distinct();
            var missingOfferIds = neededOfferIds.Where(id => !existingOfferIds.Contains(id));

            //NOTE: this _should_ be okay since the CampSum extracter just makes one call to Cake, so that's done by now
            DACampSumLoader.QuickETL_Offers(missingOfferIds);
        }

        public static void AddMissingAffiliates(List<EventConversion> items)
        {
            int[] existingAffIds;
            using (var db = new DAContext())
            {
                existingAffIds = db.Affiliates.Select(x => x.AffiliateId).ToArray();
            }
            var neededAffIds = items.Select(cs => cs.SourceAffiliate.SourceAffiliateId).Distinct();
            var missingAffIds = neededAffIds.Where(id => !existingAffIds.Contains(id));

            // ?Could just use the SourceAffiliateId and SourceAffiliateName?
            // ?future: may be interested in other attributes?
            DACampSumLoader.QuickETL_Affiliates(missingAffIds);
        }

        public static void AddUpdateDependentEvents(List<EventConversion> items)
        {
            using (var db = new DAContext())
            {
                var eventTuples = items.Select(x => Tuple.Create(x.EventInfo.EventId, x.EventInfo.EventName)).Distinct();
                foreach (var tuple in eventTuples)
                {
                    int id = tuple.Item1;
                    string name = tuple.Item2;
                    var existing = db.Events.Find(id);
                    if (existing == null)
                    {
                        db.Events.Add(new DirectAgents.Domain.Entities.Cake.Event { Id = id, Name = name });
                        Logger.Info("Saving new Event: {0} ({1})", name, id);
                        db.SaveChanges();
                    }
                    else if (existing.Name != name)
                    {
                        existing.Name = name;
                        Logger.Info("Saving updated Event name: {0} ({1})", name, id);
                        db.SaveChanges();
                    }
                }
            }
        }
        public static void AddUpdateDependentPriceFormats(List<EventConversion> items)
        {
            using (var db = new DAContext())
            {
                var pfTuples = items.Select(x => Tuple.Create(x.PriceFormat.PriceFormatId, x.PriceFormat.PriceFormatName)).Distinct();
                foreach (var tuple in pfTuples)
                {
                    int id = tuple.Item1;
                    string name = tuple.Item2;
                    var existing = db.PriceFormats.Find(id);
                    if (existing == null)
                    {
                        db.PriceFormats.Add(new DirectAgents.Domain.Entities.Cake.PriceFormat { Id = id, Name = name });
                        Logger.Info("Saving new PriceFormat: {0} ({1})", name, id);
                        db.SaveChanges();
                    }
                    else if (existing.Name != name)
                    {
                        existing.Name = name;
                        Logger.Info("Saving updated PriceFormat name: {0} ({1})", name, id);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
