using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDadPreviewLoader : Loader<TDad>
    {
        private readonly int accountId; // only used in AddUpdateDependentTDads()
        private Dictionary<string, int> tdAdIdLookupByEidAndName = new Dictionary<string, int>();

        public TDadPreviewLoader(int accountId = -1)
        {
            this.accountId = accountId;
        }

        protected override int Load(List<TDad> items)
        {
            Logger.Info("Loading {0} DA-TD AdPreviews..", items.Count);
            AddUpdateDependentTDads(items);
            return items.Count;
        }

        public void AddUpdateDependentTDads(List<TDad> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    string eidAndName = item.ExternalId + item.Name;
                    if (tdAdIdLookupByEidAndName.ContainsKey(eidAndName))
                        continue; // already encountered this TDad

                    IQueryable<TDad> tdAdsInDb = null;
                    if (!string.IsNullOrWhiteSpace(item.ExternalId))
                    {
                        // See if a TDad with that ExternalId exists
                        tdAdsInDb = db.TDads.Where(a => a.AccountId == accountId && a.ExternalId == item.ExternalId);
                        if (!tdAdsInDb.Any())
                            tdAdsInDb = null;
                    }
                    if (tdAdsInDb == null)
                    {
                        // Check by TDad name
                        tdAdsInDb = db.TDads.Where(s => s.AccountId == accountId && s.Name == item.Name);
                    }

                    if (!tdAdsInDb.Any())
                    {   // TDad doesn't exist in the db; so create it and put an entry in the lookup
                        db.TDads.Add(item);
                        db.SaveChanges();
                        Logger.Info("Saved new TDad: {0} ({1}), ExternalId={2}", item.Name, item.Id, item.ExternalId);
                        tdAdIdLookupByEidAndName[eidAndName] = item.Id;
                    }
                    else
                    {   // Update & put existing TDad in the lookup
                        // There should only be one matching TDad in the db, but just in case...
                        foreach (var tdAd in tdAdsInDb)
                        {
                            if (!string.IsNullOrWhiteSpace(item.ExternalId))
                                tdAd.ExternalId = item.ExternalId;
                            if (!string.IsNullOrWhiteSpace(item.Name))
                                tdAd.Name = item.Name;
                            if (!string.IsNullOrEmpty(item.Url))
                                tdAd.Url = item.Url;
                            if (item.Width != 0)
                                tdAd.Width = item.Width;
                            if (item.Height != 0)
                                tdAd.Height = item.Height;
                            // other properties...
                        }
                        int numUpdates = db.SaveChanges();
                        if (numUpdates > 0)
                        {
                            Logger.Info("Updated TDad: {0}, Eid={1}", item.Name, item.ExternalId);
                            if (numUpdates > 1)
                                Logger.Warn("Multiple entities in db ({0})", numUpdates);
                        }
                        tdAdIdLookupByEidAndName[eidAndName] = tdAdsInDb.First().Id;
                    }
                }
            }
        }

    }
}
