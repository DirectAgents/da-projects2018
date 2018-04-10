using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class OfferDailySummariesLoader : Loader<OfferDailySummary>
    {
        protected override int Load(List<OfferDailySummary> items)
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
                    var pk1 = item.OfferId;
                    var pk2 = item.DeleteDate ?? source.Date;

                    var target = db.Set<ClientPortal.Data.Contexts.OfferDailySummary>().Find(pk1, pk2);

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
                            target = new ClientPortal.Data.Contexts.OfferDailySummary
                            {
                                offer_id = pk1,
                                date = pk2
                            };
                            db.OfferDailySummaries.Add(target);
                            added++;
                        }
                        else
                        {
                            updated++;
                        }

                        target.views = source.Views;
                        target.clicks = source.Clicks;
                        target.click_thru = source.ClickThru;
                        target.conversions = source.Conversions;
                        target.paid = source.Paid;
                        target.sellable = source.Sellable;
                        target.conversion_rate = source.ConversionRate;
                        target.cpl = source.CPL;
                        target.cost = source.Cost;
                        target.rpt = source.RPT;
                        target.revenue = source.Revenue;
                        target.margin = source.Margin;
                        target.profit = source.Profit;
                        target.epc = source.EPC;
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