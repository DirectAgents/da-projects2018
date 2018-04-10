using System;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.DBM;

namespace DirectAgents.Domain.Abstract
{
    public interface ITDRepository : IDisposable
    {
        void SaveChanges();

        // AdRoll
        Advertisable Advertisable(string eid);
        IQueryable<Advertisable> Advertisables();
        IQueryable<Ad> AdRoll_Ads(int? advId = null, string advEid = null);
        IQueryable<AdDailySummary> AdRoll_AdDailySummaries(int? advertisableId, int? adId, DateTime? startDate, DateTime? endDate);
        TDRawStat GetAdRollStat(Ad ad, DateTime? startDate, DateTime? endDate);

        // DBM
        InsertionOrder InsertionOrder(int ioID);
        IQueryable<InsertionOrder> InsertionOrders();
        //IQueryable<Creative> DBM_Creatives(int? ioID);
        IQueryable<CreativeDailySummary> DBM_CreativeDailySummaries(DateTime? startDate, DateTime? endDate, int? ioID = null, int? creativeID = null);
        IQueryable<TDRawStat> GetDBMStatsByCreative(int ioID, DateTime? startDate, DateTime? endDate);
        TDRawStat GetDBMStat(Creative creative, DateTime? startDate, DateTime? endDate);
    }
}
