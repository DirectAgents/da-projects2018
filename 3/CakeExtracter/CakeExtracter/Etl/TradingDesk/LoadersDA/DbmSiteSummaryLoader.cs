using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class DbmSiteSummaryLoader : Loader<DbmRowBase>
    {
        private TDSiteSummaryLoader siteSummaryLoader;
        private Dictionary<string, int> siteIdLookupByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, int> accountIdLookupByIOid = new Dictionary<int, int>();

        public DbmSiteSummaryLoader()
        {
            this.siteSummaryLoader = new TDSiteSummaryLoader();
        }

        protected override int Load(List<DbmRowBase> items)
        {
            siteIdLookupByName.Clear(); // because this could get pretty big (lots of sites)

            Logger.Info("Loading {0} SiteDailySummaries..", items.Count);
            AddUpdateDependentSites(items);
            AddUpdateDependentAccounts(items);
            var ssItems = items.Select(i => CreateSiteSummary(i, siteIdLookupByName[((DbmRowWithSite)i).Site], accountIdLookupByIOid[i.InsertionOrderID])).ToList();
            var count = siteSummaryLoader.UpsertDailySummaries(ssItems);
            return count;
        }

        public static SiteSummary CreateSiteSummary(DbmRowBase item, int siteId, int accountId)
        {
            var sSum = new SiteSummary
            {
                SiteId = siteId,
                AccountId = accountId,
                Date = item.Date,
                Impressions = int.Parse(item.Impressions),
                Clicks = int.Parse(item.Clicks),
                PostClickConv = (int)decimal.Parse(item.PostClickConversions),
                PostViewConv = (int)decimal.Parse(item.PostViewConversions),
                Cost = decimal.Parse(item.Revenue)
            };
            return sSum;
        }

        private void AddUpdateDependentSites(List<DbmRowBase> items)
        {
            var siteNames = items.Select(i => ((DbmRowWithSite)i).Site).Distinct();
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

        public void AddUpdateDependentAccounts(List<DbmRowBase> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            DbmDailySummaryLoader.AddUpdateAccounts(ioTuples, accountIdLookupByIOid);
        }

    }
}
