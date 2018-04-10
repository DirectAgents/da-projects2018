using System;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.DBM;

namespace DirectAgents.Domain.Concrete
{
    public partial class TDRepository : ITDRepository, IDisposable
    {
        private ClientPortalProgContext context;

        public TDRepository(ClientPortalProgContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        #region AdRoll

        public Advertisable Advertisable(string eid)
        {
            return context.Advertisables.Where(a => a.Eid == eid).SingleOrDefault();
        }
        public IQueryable<Advertisable> Advertisables()
        {
            return context.Advertisables;
        }

        public IQueryable<Ad> AdRoll_Ads(int? advId = null, string advEid = null)
        {
            var ads = context.AdRollAds.AsQueryable();
            if (advId.HasValue)
                ads = ads.Where(a => a.AdvertisableId == advId.Value);
            if (!string.IsNullOrWhiteSpace(advEid))
                ads = ads.Where(a => a.Advertisable.Eid == advEid);
            return ads;
        }

        public IQueryable<AdDailySummary> AdRoll_AdDailySummaries(int? advertisableId, int? adId, DateTime? startDate, DateTime? endDate)
        {
            var ads = context.AdRollAdDailySummaries.AsQueryable();
            if (advertisableId.HasValue) // TODO: check what query this translates to...
                ads = ads.Where(a => a.Ad.AdvertisableId == advertisableId.Value);
            if (adId.HasValue)
                ads = ads.Where(a => a.AdId == adId.Value);
            if (startDate.HasValue)
                ads = ads.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                ads = ads.Where(a => a.Date <= endDate.Value);
            return ads;
        }

        public TDRawStat GetAdRollStat(Ad ad, DateTime? startDate, DateTime? endDate)
        {
            var stat = new TDRawStat
            {
                Name = ad.Name
            };
            var ads = AdRoll_AdDailySummaries(null, ad.Id, startDate, endDate);
            if (ads.Any())
            {
                stat.Impressions = ads.Sum(a => a.Impressions);
                stat.Clicks = ads.Sum(a => a.Clicks);
                stat.PostClickConv = ads.Sum(a => a.CTC);
                stat.PostViewConv = ads.Sum(a => a.VTC);
                stat.Cost = Math.Round(ads.Sum(a => a.Cost), 2);
                //stat.Prospects = ads.Sum(a => a.Prospects);
            }
            return stat;
        }

        #endregion
        #region DBM

        public InsertionOrder InsertionOrder(int ioID)
        {
            return context.InsertionOrders.Find(ioID);
        }
        public IQueryable<InsertionOrder> InsertionOrders()
        {
            return context.InsertionOrders;
        }

        //public IQueryable<Creative> DBM_Creatives(int? ioID)
        //{
        //    var creatives = context.Creatives.AsQueryable();
        //    if (ioID.HasValue)
        //        creatives = creatives.Where(c => c.InsertionOrderID == ioID);
        //    return creatives;
        //}

        public IQueryable<CreativeDailySummary> DBM_CreativeDailySummaries(DateTime? startDate, DateTime? endDate, int? ioID = null, int? creativeID = null)
        {
            var cds = context.DBMCreativeDailySummaries.AsQueryable();
            if (ioID.HasValue)
                cds = cds.Where(c => c.InsertionOrderID == ioID.Value);
            if (creativeID.HasValue)
                cds = cds.Where(c => c.CreativeID == creativeID.Value);
            if (startDate.HasValue)
                cds = cds.Where(c => c.Date >= startDate.Value);
            if (endDate.HasValue)
                cds = cds.Where(c => c.Date <= endDate.Value);
            return cds;
        }

        public IQueryable<TDRawStat> GetDBMStatsByCreative(int ioID, DateTime? startDate, DateTime? endDate)
        {
            var cds = DBM_CreativeDailySummaries(startDate, endDate, ioID: ioID);
            var groups = cds.GroupBy(c => c.Creative);
            var stats = groups.Select(g => new TDRawStat
            {
                Name = g.Key.Name,
                Impressions = g.Sum(c => c.Impressions),
                Clicks = g.Sum(c => c.Clicks),
                PostClickConv = g.Sum(c => c.PostClickConv),
                PostViewConv = g.Sum(c => c.PostViewConv),
                Cost = g.Sum(c => c.Revenue)
            });
            return stats;
        }

        public TDRawStat GetDBMStat(Creative creative, DateTime? startDate, DateTime? endDate)
        {
            var stat = new TDRawStat
            {
                Name = creative.Name
            };
            var cds = DBM_CreativeDailySummaries(startDate, endDate, creativeID: creative.ID);
            if (cds.Any())
            {
                stat.Impressions = cds.Sum(c => c.Impressions);
                stat.Clicks = cds.Sum(c => c.Clicks);
                stat.PostClickConv = cds.Sum(c => c.PostClickConv);
                stat.PostViewConv = cds.Sum(c => c.PostViewConv);
                stat.Cost = cds.Sum(c => c.Revenue);
            }
            return stat;
        }
        #endregion

        // ---

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    context.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
