using System;
using System.Collections.Generic;
using System.Linq;
using ClientPortal.Data.DTOs.TD;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.AdRoll;
using ClientPortal.Data.Entities.TD.DBM;

namespace ClientPortal.Data.Contracts
{
    public interface ITDRepository : IDisposable
    {
        void SaveChanges();

        IQueryable<AdRollProfile> AdRollProfiles();
        int MaxAdRollProfileId();
        AdRollProfile GetAdRollProfile(int id);
        void SaveAdRollProfile(AdRollProfile arProfile);

        IQueryable<InsertionOrder> InsertionOrders();
        InsertionOrder GetInsertionOrder(int insertionOrderID);
        void SaveInsertionOrder(InsertionOrder insertionOrder);

        bool CreateAccountForInsertionOrder(int insertionOrderID);

        IQueryable<TradingDeskAccount> TradingDeskAccounts();
        int MaxTradingDeskAccountId();
        TradingDeskAccount GetTradingDeskAccount(int tradingDeskAccountId);
        bool SaveTradingDeskAccount(TradingDeskAccount tdAccount);
        bool CreateTradingDeskAccount(TradingDeskAccount tdAccount);

        // --- TDStatsRepository ---
        IEnumerable<StatsSummary> GetDailyStatsSummaries(DateTime? start, DateTime? end, TradingDeskAccount tdAccount);
        IEnumerable<CreativeStatsSummary> GetCreativeStatsSummaries(DateTime? start, DateTime? end, TradingDeskAccount tdAccount);
        IEnumerable<RangeStat> GetWeekStats(TradingDeskAccount tdAccount, int numWeeks, DateTime? end);
        IEnumerable<RangeStat> GetMonthStats(TradingDeskAccount tdAccount, int numMonths, DateTime end);

        StatsRollup AdRollStatsRollup(int profileId);

        IQueryable<UserListRun> UserListRuns(int insertionOrderID);
    }
}
