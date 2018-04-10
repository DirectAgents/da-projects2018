using System;
using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAOfferSummaryLoader : Loader<OfferSummary>
    {
        private readonly DateTime date;

        public DAOfferSummaryLoader(DateTime date)
        {
            this.date = date;
        }

        protected override int Load(List<OfferSummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;

            using (var db = new DirectAgents.Domain.Contexts.DAContext())
            {
                foreach (var item in items)
                {
                    var pk1 = item.Offer.OfferId;
                    // (pk2 = date)
                    var target = db.Set<DirectAgents.Domain.Entities.Cake.OfferDailySummary>().Find(pk1, date);
                    if (target == null)
                    {
                        target = new DirectAgents.Domain.Entities.Cake.OfferDailySummary
                        {
                            OfferId = pk1,
                            Date = date
                        };
                        db.OfferDailySummaries.Add(target);
                        added++;
                    }
                    else
                    {
                        updated++;
                    }

                    //TODO: use AutoMapper (set EntityState to Detached, then Modified)

                    target.Views = item.Views;
                    target.Clicks = item.Clicks;
                    target.Conversions = item.Conversions;
                    target.Paid = item.Paid;
                    target.Sellable = item.Sellable;
                    target.Revenue = item.Revenue;
                    target.Cost = item.Cost;

                    loaded++;
                }
                Logger.Info("Loading {0} DailySummaries ({1} updates, {2} additions)", loaded, updated, added);
                db.SaveChanges();
            }
            return loaded;
        }
    }
}
