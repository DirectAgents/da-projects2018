using CakeExtracter.Common;
using ClientPortal.Data.Contexts;
using LocalConnex;
using System.Collections.Generic;
using System.Linq;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class LocalConnexApiExtracter : Extracter<CallDailySummary>
    {
        private readonly DateRange dateRange;
        private readonly string accid;
        private readonly int minSeconds;

        private LCUtility _lcUtility;

        public LocalConnexApiExtracter(DateRange dateRange, string accid, int minSeconds)
        {
            this.dateRange = dateRange;
            this.accid = accid;
            this.minSeconds = minSeconds;
            _lcUtility = new LCUtility(m => Logger.Info(m), m => Logger.Warn(m));
        }

        protected override void Extract()
        {
            Logger.Info("Extracting CallDailySummaries from LocalConnex API for {0} from {1:d} to {2:d}",
                        this.accid, this.dateRange.FromDate, this.dateRange.ToDate);

            var dailySummaries = EnumerateDailySummaries();
            Add(dailySummaries);
            End();
        }

        private IEnumerable<CallDailySummary> EnumerateDailySummaries()
        {
            foreach (var date in dateRange.Dates)
            {
                var calls = _lcUtility.GetCalls(accid, date);
                var campaignGroups = calls.Where(c => c.call_duration >= minSeconds).GroupBy(c => c.cmpid);
                foreach (var campaignGroup in campaignGroups)
                {
                    var cds = new CallDailySummary
                    {
                        Date = date,
                        LCcmpid = campaignGroup.Key,
                        Calls = campaignGroup.Select(cg => cg.caller_number).Distinct().Count()
                    };
                    yield return cds;
                }
            }
        }

    }
}
