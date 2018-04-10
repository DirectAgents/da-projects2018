using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class DailySummariesLoader : Loader<OfferAffiliateDailySummary>
    {
        protected override int Load(List<OfferAffiliateDailySummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            var deleted = 0;
            var alreadyDeleted = 0;
            using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
            {
                foreach (var item in items)
                {
                    var source = item.DailySummary;
                    var pk1 = item.DeleteDate ?? source.Date;
                    var pk2 = item.OfferId;
                    var pk3 = item.AffiliateId;

                    var target = db.Set<ClientPortal.Data.Contexts.DailySummary>().Find(pk1, pk2, pk3);

                    if (item.DeleteDate.HasValue) // Marked for deletion
                    {
                        if (target == null)
                        {
                            alreadyDeleted++;
                        }
                        else
                        {
                            db.DailySummaries.Remove(target);
                            deleted++;
                        }

                    }
                    else // Marked for addition (or updating)
                    {
                        if (target == null)
                        {
                            target = new ClientPortal.Data.Contexts.DailySummary
                                         {
                                             Date = pk1,
                                             OfferId = pk2,
                                             AffiliateId = pk3
                                         };
                            db.DailySummaries.Add(target);
                            added++;
                        }
                        else
                        {
                            updated++;
                        }

                        target.Views = source.Views;
                        target.Clicks = source.Clicks;
                        target.Conversions = source.Conversions;
                        target.Paid = source.Paid;
                        target.Sellable = source.Sellable;
                        target.Cost = source.Cost;
                        target.Revenue = source.Revenue;
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