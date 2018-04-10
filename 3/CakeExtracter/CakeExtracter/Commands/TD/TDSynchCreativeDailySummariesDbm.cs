using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;
using ClientPortal.Data.Entities.TD;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class TDSynchCreativeDailySummariesDbm : ConsoleCommand
    {
        public int? TradingDeskAccountId { get; set; }
        //public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            TradingDeskAccountId = null;
            //StartDate = null;
            EndDate = null;
        }

        public TDSynchCreativeDailySummariesDbm()
        {
            IsCommand("tdSynchCreativeDailySummariesDbm", "synch CreativeDailySummaries for DBM Report");
            HasOption<int>("t|tradingDeskAccountId=", "TradingDeskAccount Id (default = all)", c => TradingDeskAccountId = c);
            //HasOption<DateTime>("s|startDate=", "Start Date (default is 2 days ago)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            // Note: endDate is the last day of the desired stats (a report goes back one month)
        }

        public override int Execute(string[] remainingArguments)
        {
            //string bucketName = "151075984680687222131410283081521_report"; // Betterment_creative

            //var twoDaysAgo = DateTime.Today.AddDays(-2);
            //var yesterday = DateTime.Today.AddDays(-1);
            //var dateRange = new DateRange(StartDate ?? twoDaysAgo, EndDate ?? yesterday);

            // Note: The reportDate will be one day after the endDate of the desired stats
            DateTime endDate = EndDate ?? DateTime.Today.AddDays(-1);
            var reportDate = endDate.AddDays(1);

            var buckets = GetBuckets();

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, byCreative: true);
            var loader = new DbmCreativeDailySummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }

        public IEnumerable<string> GetBuckets()
        {
            var buckets = new List<string>();
            if (TradingDeskAccountId.HasValue)
            {
                using (var db = new TDContext())
                {
                    var tda = db.TradingDeskAccounts.Find(TradingDeskAccountId.Value);
                    if (tda != null)
                    {
                        foreach (var io in tda.InsertionOrders)
                        {
                            if (!string.IsNullOrWhiteSpace(io.Bucket))
                                buckets.Add(io.Bucket);
                        }
                    }
                    //Note: this excludes IOs that don't have a TradingDeskAccountId or a Bucket
                }
            }
            else
            {
                buckets.Add(ConfigurationManager.AppSettings["DBM_AllCreativeBucket"]);
            }
            return buckets;
        }

    }
}
