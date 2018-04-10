using System.Collections.Generic;
using CakeExtracter.CakeMarketingApi.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DACampLoader : Loader<Campaign>
    {
        protected override int Load(List<Campaign> items)
        {
            var loaded = 0;
            var added = 0;
            var updated = 0;
            using (var db = new DAContext())
            {
                foreach (var item in items)
                {
                    var target = db.Set<Camp>().Find(item.CampaignId);
                    if (target == null)
                    {
                        target = new Camp
                        {
                            CampaignId = item.CampaignId
                        };
                        item.CopyValuesTo(target);
                        db.Camps.Add(target);
                        added++;
                    }
                    else
                    {
                        item.CopyValuesTo(target);
                        updated++;
                    }
                    loaded++;
                }
                Logger.Info("Loading {0} Camps ({1} updates, {2} additions)", loaded, updated, added);
                db.SaveChanges();
            }
            return loaded;
        }
    }
}
