using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Common;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DatabaseStrategyToDailySummaryExtracter : Extracter<DailySummary>
    {
        protected readonly DateRange dateRange;
        protected readonly int accountId;

        public DatabaseStrategyToDailySummaryExtracter(DateRange dateRange, int accountId)
        {
            this.dateRange = dateRange;
            this.accountId = accountId;
        }
        protected override void Extract()
        {
            Logger.Info("Extracting DailySummaries from Db for ({0}) from {1:d} to {2:d}",
                this.accountId, this.dateRange.FromDate, this.dateRange.ToDate);
            var items = EnumerateRows();
            Add(items);
            End();
        }
        public IEnumerable<DailySummary> EnumerateRows()
        {
            List<DailySummary> daySums = new List<DailySummary>();
            using (var db = new ClientPortalProgContext())
            {
                var stratSums = db.StrategySummaries.Where(x => x.Strategy.AccountId == accountId && x.Date >= dateRange.FromDate && x.Date <= dateRange.ToDate);
                var dayGroups = stratSums.GroupBy(x => x.Date);
                foreach (var g in dayGroups)
                {
                    var daySum = new DailySummary
                    {
                        Date = g.Key,
                        Cost = g.Sum(x => x.Cost),
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        AllClicks = g.Sum(x => x.AllClicks),
                        PostClickConv = g.Sum(x => x.PostClickConv),
                        PostClickRev = g.Sum(x => x.PostClickRev),
                        PostViewConv = g.Sum(x => x.PostViewConv),
                        PostViewRev = g.Sum(x => x.PostViewRev)
                    };
                    daySums.Add(daySum);
                }
            }
            return daySums;
        }
    }
}
