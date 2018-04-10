using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDSiteSummaryLoader : Loader<SiteSummary>
    {
        private readonly int accountId;
        private Dictionary<string, int> siteIdLookupByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public TDSiteSummaryLoader(int accountId = -1)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<SiteSummary> items)
        {
            siteIdLookupByName.Clear(); // because this could get pretty big (lots of sites)

            Logger.Info("Loading {0} DA-TD SiteSummaries..", items.Count);
            AddUpdateDependentSites(items);
            AssignIdsToItems(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        public void AssignIdsToItems(List<SiteSummary> items)
        {
            foreach (var item in items)
            {
                item.AccountId = accountId;
                if (siteIdLookupByName.ContainsKey(item.SiteName))
                    item.SiteId = siteIdLookupByName[item.SiteName];
            }
        }

        public int UpsertDailySummaries(List<SiteSummary> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var deletedCount = 0;
            var alreadyDeletedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                //var itemSiteIds = items.Select(i => i.SiteId).Distinct().ToArray();
                //var siteIdsInDb = db.Sites.Select(s => s.Id).Where(i => itemSiteIds.Contains(i)).ToArray();
                // (assume all the site ids have been assigned to something that exists in the db)

                foreach (var item in items)
                {
                    var target = db.Set<SiteSummary>().Find(item.Date, item.SiteId, item.AccountId);
                    if (target == null)
                    {
                        if (item.AllZeros())
                        {
                            alreadyDeletedCount++;
                        }
                        else
                        {
                            //if (siteIdsInDb.Contains(item.SiteId))
                            //{
                                db.SiteSummaries.Add(item);
                                addedCount++;
                            //}
                            //else
                            //{
                            //    Logger.Warn("Skipping load of item. Site with id {0} does not exist.", item.SiteId);
                            //    skippedCount++;
                            //}
                        }
                    }
                    else // SiteSummary already exists
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            if (!item.AllZeros())
                            {
                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(item, target);
                                entry.State = EntityState.Modified;
                                updatedCount++;
                            }
                            else
                            {
                                entry.State = EntityState.Deleted;
                                deletedCount++;
                            }
                        }
                        else
                        {
                            Logger.Warn("Encountered duplicate for {0:d} - Site {1} ({2}), Account {3}", item.Date, item.SiteId, item.SiteName, item.AccountId);
                            duplicateCount++;
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} SiteSummaries ({1} updates, {2} additions, {3} duplicates, {4} deleted, {5} already-deleted, {6} skipped)",
                            itemCount, updatedCount, addedCount, duplicateCount, deletedCount, alreadyDeletedCount, skippedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }

        private void AddUpdateDependentSites(List<SiteSummary> items)
        {
            var siteNames = items.Select(i => i.SiteName).Distinct();
            AddUpdateDependentSites(siteNames, siteIdLookupByName);
        }
        public static void AddUpdateDependentSites(IEnumerable<string> siteNames, Dictionary<string, int> siteIdLookupByName)
        {
            using (var db = new ClientPortalProgContext())
            {
                foreach (var siteName in siteNames)
                {
                    if (siteIdLookupByName.ContainsKey(siteName))
                        continue; // already encountered this site

                    var sites = db.Sites.Where(s => s.Name == siteName);
                    if (!sites.Any())
                    {
                        var site = new Site
                        {
                            Name = siteName
                        };
                        db.Sites.Add(site);
                        db.SaveChanges();
                        Logger.Info("Saved new Site: {0} ({1})", site.Name, site.Id);
                        siteIdLookupByName[siteName] = site.Id;
                    }
                    else
                    {
                        var site = sites.First(); // there shouldn't be more than one with the same name, but...
                        siteIdLookupByName[siteName] = site.Id;
                    }
                }
            }
        }

    }
}
