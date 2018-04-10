using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchDBMStatsOld : ConsoleCommand
    {
        public static int RunStatic(int? insertionOrderID = null, DateTime? endDate = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchDBMStatsOld
            {
                InsertionOrderID = insertionOrderID,
                EndDate = endDate
            };
            return cmd.Run();
        }

        public int? InsertionOrderID { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysPerReport { get; set; }

        public override void ResetProperties()
        {
            InsertionOrderID = null;
            EndDate = null;
            DaysPerReport = 0; // 0 means determine from the loader
            // (Determines how far back to go when updating TD tables from DBM tables)
        }

        // Loads DBM tables (CreativeDailySummaries) from the DBM API.  Then updates TD tables (one daySum per account) from the DBM tables.
        public DASynchDBMStatsOld()
        {
            IsCommand("daSynchDBMStatsOld", "synch DBM Stats - the old way");
            HasOption<int>("i|insertionOrderID=", "InsertionOrder ID (default = all)", c => InsertionOrderID = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            // Note: endDate is the last day of the desired stats (a report goes back X days)
            HasOption<int>("d|daysPerReport=", "Days Per Report (0 (default) means determine from loader", c => DaysPerReport = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            // Note: The reportDate will be one day after the endDate of the desired stats
            DateTime endDate = EndDate ?? DateTime.Today.AddDays(-1);
            var reportDate = endDate.AddDays(1);

            var buckets = GetBuckets();

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, byCreative: true);
            var loader = new DBMCreativeDailySummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            var updateStart = endDate.AddDays(1 - DaysPerReport);
            if (DaysPerReport <= 0 && loader.EarliestDate.HasValue)
                updateStart = loader.EarliestDate.Value;
            var updateDateRange = new DateRange(updateStart, endDate);
            UpdateTDTablesFromDBMTables(updateDateRange, InsertionOrderID);

            return 0;
        }

        public IEnumerable<string> GetBuckets()
        {
            var buckets = new List<string>();
            if (InsertionOrderID.HasValue)
            {
                using (var db = new ClientPortalProgContext())
                {
                    var IOs = db.InsertionOrders.Where(io => io.ID == InsertionOrderID.Value);
                    foreach (var io in IOs) // should be just one
                    {
                        if (!string.IsNullOrWhiteSpace(io.Bucket))
                            buckets.Add(io.Bucket);
                    }
                }
            }
            else
            {
                buckets.Add(ConfigurationManager.AppSettings["DBM_AllCreativeBucket"]);
            }
            return buckets;
        }

        // Note this will update DailySummaries for all insertion orders (Accounts) with CreativeDailySummary stats in the specified range.
        // (unless insertionOrderID is specified)
        public static void UpdateTDTablesFromDBMTables(DateRange dateRange, int? insertionOrderID)
        {
            Logger.Info("Updating (External) Accounts and DailySummaries for dateRange {0:d} to {1:d}", dateRange.FromDate, dateRange.ToDate);

            using (var db = new ClientPortalProgContext())
            {
                var dbmPlatformId = db.Platforms.Where(p => p.Code == Platform.Code_DBM).First().Id;
                var dbmAccounts = db.ExtAccounts.Where(a => a.PlatformId == dbmPlatformId);
                var dbmExternalIds = dbmAccounts.Select(a => a.ExternalId).ToList();

                // 1) InsertionOrders (with stats in this dateRange) -> add Account(s) if necessary
                var cdSums = db.DBMCreativeDailySummaries.Where(cds => cds.Date >= dateRange.FromDate && cds.Date <= dateRange.ToDate);
                if (insertionOrderID.HasValue)
                    cdSums = cdSums.Where(cds => cds.InsertionOrderID == insertionOrderID.Value);
                var insertionOrders = cdSums.Select(cds => cds.InsertionOrder).Distinct().ToList();
                foreach (var io in insertionOrders)
                {
                    if (!dbmExternalIds.Contains(io.ID.ToString()))
                    { // add
                        var newAccount = new ExtAccount
                        {
                            PlatformId = dbmPlatformId,
                            ExternalId = io.ID.ToString(),
                            Name = io.Name
                        };
                        db.ExtAccounts.Add(newAccount);
                        Logger.Info("Adding new ExtAccount from InsertionOrder: {0} ({1})", io.Name, io.ID);
                    }
                    // Note: skipping update
                }
                db.SaveChanges();

                // 2) Add/Update DailySummaries
                foreach (var io in insertionOrders)
                {
                    var accountId = db.ExtAccounts.Where(a => a.ExternalId == io.ID.ToString()).First().Id;

                    var cdSumsForIO = cdSums.Where(cds => cds.InsertionOrderID == io.ID);
                    var statDates = cdSumsForIO.Select(cds => cds.Date).Distinct().ToList();
                    foreach (var date in dateRange.Dates)
                    {
                        var existingDS = db.DailySummaries.Find(date, accountId);

                        if (!statDates.Contains(date))
                        { // No stats. Make sure there's no td.DailySummary for this date
                            if (existingDS != null)
                                db.DailySummaries.Remove(existingDS);
                        }
                        else
                        { // Add or update tdDailySummary
                            var cds1day = cdSumsForIO.Where(cds => cds.Date == date);
                            var newDS = new DailySummary
                            {
                                Date = date,
                                AccountId = accountId,
                                Impressions = cds1day.Sum(cds => cds.Impressions),
                                Clicks = cds1day.Sum(cds => cds.Clicks),
                                PostClickConv = cds1day.Sum(cds => cds.PostClickConv),
                                PostViewConv = cds1day.Sum(cds => cds.PostViewConv),
                                Cost = cds1day.Sum(cds => cds.Revenue)
                            };

                            if (existingDS == null)
                            {
                                db.DailySummaries.Add(newDS);
                            }
                            else
                            {
                                var entry = db.Entry(existingDS);
                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(newDS, existingDS);
                                entry.State = EntityState.Modified;
                            }
                        }
                    }
                    db.SaveChanges();
                } //foreach insertionOrder
            } //using db
        }

    }
}
