using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class CreativeMonthlySummariesLoader : Loader<CreativeMonthlySummary>
    {
        protected override int Load(List<CreativeMonthlySummary> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
            {
                foreach (var item in items)
                {
                    var source = item.Summary;
                    var pk1 = item.CreativeId;
                    var pk2 = source.Date;

                    var target = db.Set<ClientPortal.Data.Contexts.CreativeSummary>().Find(pk1, pk2);

                    if (target == null)
                    {
                        target = new ClientPortal.Data.Contexts.CreativeSummary
                        {
                            CreativeId = pk1,
                            Date = pk2
                        };
                        db.CreativeSummaries.Add(target);
                        added++;
                    }
                    else
                    {
                        updated++;
                    }

                    target.Views = source.Views;
                    target.Clicks = source.Clicks;
                    target.Conversions = source.Conversions;

                    loaded++;
                }

                Logger.Info("Loading {0} CreativeSummaries ({1} updates, {2} additions)", loaded, updated, added);

                db.SaveChanges();
            }

            return loaded;
        }
    }
}
