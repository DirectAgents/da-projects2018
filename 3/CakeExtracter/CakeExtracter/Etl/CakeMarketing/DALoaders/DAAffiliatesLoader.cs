using System.Collections.Generic;
using System.Data.Entity;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAAffiliatesLoader : Loader<CakeExtracter.CakeMarketingApi.Entities.Affiliate>
    {
        public DAAffiliatesLoader() { }

        protected override int Load(List<CakeMarketingApi.Entities.Affiliate> items)
        {
            Logger.Info("Synching {0} affiliates...", items.Count);
            using (var db = new DAContext())
            {
                foreach (var item in items)
                {
                    var affiliate = db.Set<Affiliate>().Find(item.AffiliateId);
                    if (affiliate == null)
                    {
                        affiliate = new Affiliate
                        {
                            AffiliateId = item.AffiliateId
                        };
                        db.Affiliates.Add(affiliate);
                    }
                    else // Affiliate found (in cache or db)
                    {
                        var entry = db.Entry(affiliate);
                        if (entry.State != EntityState.Unchanged)
                        {
                            Logger.Warn("Encountered duplicate Affiliate {0} - skipping", item.AffiliateId);
                            continue;
                        }
                    }
                    affiliate.AffiliateName = item.AffiliateName;
                }
                db.SaveChanges();
            }
            return items.Count;
        }

    }
}
