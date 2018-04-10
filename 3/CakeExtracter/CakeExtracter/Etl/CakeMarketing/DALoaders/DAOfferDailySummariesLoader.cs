using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAOfferDailySummariesLoader : Loader<OfferDailySummary>
    {
        protected override int Load(List<OfferDailySummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            var deleted = 0;
            var alreadyDeleted = 0;
            using (var db = new DirectAgents.Domain.Contexts.DAContext())
            {
                foreach (var item in items)
                {
                    var source = item.DailySummary;
                    var pk1 = item.OfferId;
                    var pk2 = item.DeleteDate ?? source.Date;

                    var target = db.Set<DirectAgents.Domain.Entities.Cake.OfferDailySummary>().Find(pk1, pk2);

                    if (item.DeleteDate.HasValue) // Marked for deletion
                    {
                        if (target == null)
                        {
                            alreadyDeleted++;
                        }
                        else
                        {
                            db.OfferDailySummaries.Remove(target);
                            deleted++;
                        }

                    }
                    else // Marked for addition (or updating)
                    {
                        if (target == null)
                        {
                            target = new DirectAgents.Domain.Entities.Cake.OfferDailySummary
                            {
                                OfferId = pk1,
                                Date = pk2
                            };
                            db.OfferDailySummaries.Add(target);
                            added++;
                        }
                        else
                        {
                            updated++;
                        }

                        //TODO: use AutoMapper

                        target.Views = source.Views;
                        target.Clicks = source.Clicks;
                        target.Conversions = source.Conversions;
                        target.Paid = source.Paid;
                        target.Sellable = source.Sellable;
                        target.Revenue = source.Revenue;
                        target.Cost = source.Cost;
                    }
                    loaded++;
                }

                Logger.Info("Loading {0} DailySummaries ({1} updates, {2} additions, {3} deletions, {4} already-deleted)", loaded, updated, added, deleted, alreadyDeleted);

                db.SaveChanges();
            }

            return loaded;
        }
    }
}
