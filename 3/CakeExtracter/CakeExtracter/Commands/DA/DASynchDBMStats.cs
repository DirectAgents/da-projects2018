using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using CakeExtracter.Bootstrappers;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.DBM;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;
using DBM;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchDBMStats : ConsoleCommand
    {
        //Note: if make a RunStatic, be sure to add 'DBM_AllSiteBucket', etc to the web.config
        public static int RunStatic(int? insertionOrderID = null, DateTime? startDate = null, DateTime? endDate = null, string statsType = null, string advertiserId = "")
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchDBMStats
            {
                InsertionOrderID = insertionOrderID,
                StartDate = startDate ?? DateTime.Today,
                EndDate = endDate ?? DateTime.Today,
                StatsType = statsType,
                AdvertiserID = advertiserId
            };
            return cmd.Run();
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Historical { get; set; }
        public string StatsType { get; set; }
        public int? AccountId { get; set; }
        public int? InsertionOrderID { get; set; }
        public string AdvertiserID { get; set; }

        private DBMUtility dbmUtility { get; set; }

        public override void ResetProperties()
        {
            StartDate = null;
            EndDate = null;
            Historical = false;
            StatsType = null;
            AccountId = null;
            InsertionOrderID = null;
            AdvertiserID = null;
        }

        public DASynchDBMStats()
        {
            IsCommand("daSynchDBMStats", "synch DBM Daily Stats - by lineitem/creative/site...");
            HasOption<DateTime>("s|startDate=", "Start Date - for conversions only (default is today & ignore end date)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            HasOption("h|Historical=", "Get historical stats (ignore endDate)", c => Historical = bool.Parse(c));
            HasOption<string>("t|statsType=", "Stats Type (default: all)", c => StatsType = c);
            HasOption<int>("a|accountId=", "Account Id (default = all)", c => AccountId = c);
            HasOption<int?>("i|insertionOrder=", "Insertion Order (default: all)", c => InsertionOrderID = c);
            HasOption<string>("v|advertiserId=", "Advertiser ID (default: all)", c => AdvertiserID = c);
        }

        private void Test()
        {
            SetupDBMUtility();
            //dbmUtility.TokenSets = new string[] { "|DBMDBM|1/VC8MQArCKHna2NmLFYg4GVcftxtgMo1p4lpw-ZeLXRo" };
            dbmUtility.Test();
            SaveTokens();
        }
        private void SetupDBMUtility()
        {
            this.dbmUtility = new DBMUtility(m => Logger.Info(m), m => Logger.Warn(m));
            GetTokens();
        }
        private void GetTokens()
        {
            // Get tokens, if any, from the database
            string[] tokenSets = Platform.GetPlatformTokens(Platform.Code_DBM);
            dbmUtility.TokenSets = tokenSets;
        }
        private void SaveTokens()
        {
            Platform.SavePlatformTokens(Platform.Code_DBM, dbmUtility.TokenSets);
        }

        public override int Execute(string[] remainingArguments)
        {
            //Test();

            SetInsertionOrderFromAccount();
            if (Historical)
                DoHistorical();
            else
                DoRegular();
            return 0;
        }
        private void SetInsertionOrderFromAccount()
        {
            if (InsertionOrderID.HasValue || !AccountId.HasValue)
                return; // skip if ioID is already specified (or no account is specified)
            using (var db = new ClientPortalProgContext())
            {
                var extAcct = db.ExtAccounts.Find(AccountId.Value);
                if (extAcct != null)
                {
                    int externalId;
                    if (int.TryParse(extAcct.ExternalId, out externalId))
                        InsertionOrderID = externalId;
                }
            }
        }

        public void DoRegular()
        {
            // Note: The reportDate will be one day after the endDate of the desired stats
            DateTime endDate = EndDate ?? DateTime.Today.AddDays(-1);
            var reportDate = endDate.AddDays(1);

            var statsType = new StatsTypeAgg(this.StatsType);

            if (statsType.Daily)
                DoETL_Daily(reportDate: reportDate);
            if (statsType.Strategy)
                DoETL_Strategy(reportDate: reportDate);
            if (statsType.Creative)
                DoETL_Creative(reportDate: reportDate);
            if (statsType.Site)
                DoETL_Site(reportDate: reportDate);

            if (statsType.Conv && !statsType.All) // don't include when getting "all" statstypes
                DoETL_Conv();
        }

        public void DoHistorical()
        {
            var statsType = new StatsTypeAgg(this.StatsType);
            if (statsType.Daily)
                DoETL_Daily(buckets: BucketNamesFromConfig("DBM_AllIOBucket_Historical"));
            if (statsType.Strategy)
                DoETL_Strategy(buckets: BucketNamesFromConfig("DBM_AllLineItemBucket_Historical"));
            if (statsType.Creative)
                DoETL_Creative(buckets: BucketNamesFromConfig("DBM_AllCreativeBucket_Historical"));
            if (statsType.Site)
                DoETL_Site(buckets: BucketNamesFromConfig("DBM_AllSiteBucket_Historical"));
            //if (statsType.Conv)
            // TODO: implement
        }
        public static IEnumerable<string> BucketNamesFromConfig(string configKey)
        {
            var configVal = ConfigurationManager.AppSettings[configKey];
            if (configVal == null)
                configVal = String.Empty;
            return configVal.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void DoETL_Daily(DateTime? reportDate = null, IEnumerable<string> buckets = null)
        {
            if (buckets == null)
                buckets = new List<string> { ConfigurationManager.AppSettings["DBM_AllIOBucket"] };

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, ioFilter: InsertionOrderID);
            var loader = new DbmDailySummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Strategy(DateTime? reportDate = null, IEnumerable<string> buckets = null)
        {
            if (buckets == null)
                buckets = new List<string> { ConfigurationManager.AppSettings["DBM_AllLineItemBucket"] };

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, byLineItem: true, ioFilter: InsertionOrderID);
            var loader = new DbmLineItemSummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Creative(DateTime? reportDate = null, IEnumerable<string> buckets = null)
        {
            if (buckets == null)
                buckets = new List<string> { ConfigurationManager.AppSettings["DBM_AllCreativeBucket"] };

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, byCreative: true, ioFilter: InsertionOrderID);
            var loader = new DbmCreativeSummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Site(DateTime? reportDate = null, IEnumerable<string> buckets = null)
        {
            if (buckets == null)
                buckets = new List<string> { ConfigurationManager.AppSettings["DBM_AllSiteBucket"] };

            var extracter = new DbmCloudStorageExtracter(reportDate, buckets, bySite: true, ioFilter: InsertionOrderID);
            var impThresholdString = ConfigurationManager.AppSettings["TD_SiteStats_ImpressionThreshold"];
            int impThreshold;
            if (int.TryParse(impThresholdString, out impThreshold))
                extracter.ImpressionThreshold = impThreshold;

            var loader = new DbmSiteSummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

        // to be tested...
        // report for each day has data from two days ago, i.e. report for 12/11/2016 will have conv data for 12/9/2016.
        public void DoETL_Conv() //null for all
        {
            var today = DateTime.Today;
            var dateRange = (StartDate == null) ? new DateRange(today, today) : new DateRange(StartDate.Value, EndDate.Value);

            if (InsertionOrderID != null)
            {
                using (var db = new ClientPortalProgContext())
                {
                    var insertionOrder = db.InsertionOrders.Where(c => c.ID == InsertionOrderID.Value).FirstOrDefault();
                    if (insertionOrder != null)
                        AdvertiserID = insertionOrder.Bucket.ToString();
                }
            }

            var advertiserIds = (AdvertiserID == "" || AdvertiserID == null) ? (BucketNamesFromConfig("DBM_AllAdvertiserIds")) : new string[] { AdvertiserID };
            int timezoneOffset = -5; // w/o daylight savings
            var convConverter = new CakeExtracter.Etl.TradingDesk.Loaders.DbmConvConverter(timezoneOffset);

            var extracter = new DbmConversionExtracter(dateRange, advertiserIds, InsertionOrderID, true);
            var loader = new DbmConvLoader(convConverter);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
    }
}
