using System;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using CakeExtracter.Etl;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchTDDailySummaries : ConsoleCommand
    {
        public static int RunStatic(int accountId, StreamReader streamReader, string statsType = null, DateTime? statsDate = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchTDDailySummaries
            {
                AccountId = accountId,
                StreamReader = streamReader,
                StatsType = statsType,
                StatsDate = statsDate
            };
            int result = cmd.Run();
            return result;
        }

        public int AccountId { get; set; }
        public StreamReader StreamReader { get; set; }
        public string FilePath { get; set; }
        public string StatsType { get; set; }
        public DateTime? StatsDate { get; set; } // optional

        public override void ResetProperties()
        {
            AccountId = 0;
            StreamReader = null;
            FilePath = null;
            StatsType = null;
            StatsDate = null;
        }

        public DASynchTDDailySummaries()
        {
            IsCommand("daSynchTDDailySummaries", "synch daily summaries via file upload");
            HasRequiredOption<int>("a|accountId=", "Account ID", c => AccountId = c);
            HasOption<string>("f|filePath=", "CSV filepath", c => FilePath = c);
            HasOption<string>("t|statsType=", "Stats Type (default: all)", c => StatsType = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var account = GetAccount(AccountId);
            if (account != null)
            {
                ColumnMapping mapping = account.Platform.PlatColMapping;
                if (mapping == null)
                    mapping = ColumnMapping.CreateDefault();

                //TESTING!
                mapping.AdSetName = "Line Item";
                mapping.AdSetEid = "Line Item ID";
                //TODO: allow saving to db

                var statsType = new StatsTypeAgg(this.StatsType);

                if (statsType.Daily)
                    DoETL_Daily(mapping);
                if (statsType.Strategy)
                    DoETL_Strategy(mapping);
                if (statsType.AdSet)
                    DoETL_AdSet(mapping);
                if (statsType.Creative)
                    DoETL_Creative(mapping);
                if (statsType.Site)
                    DoETL_Site(mapping);
                if (statsType.Conv)
                    DoETL_Conv();
            }
            else
            {
                Logger.Warn("ExtAccount ({0}) not found. Skipping ETL.", AccountId);
            }
            return 0;
        }

        public void DoETL_Daily(ColumnMapping mapping)
        {
            var extracter = new TDDailySummaryExtracter(mapping, streamReader: StreamReader, csvFilePath: FilePath);
            var loader = new TDDailySummaryLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Strategy(ColumnMapping mapping)
        {
            var extracter = new TDStrategySummaryExtracter(mapping, streamReader: StreamReader, csvFilePath: FilePath);
            var loader = new TDStrategySummaryLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_AdSet(ColumnMapping mapping)
        {
            var extracter = new TDAdSetSummaryExtracter(mapping, streamReader: StreamReader, csvFilePath: FilePath);
            var loader = new TDAdSetSummaryLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Creative(ColumnMapping mapping)
        {
            var extracter = new TDadSummaryExtracter(mapping, streamReader: StreamReader, csvFilePath: FilePath);
            var loader = new TDadSummaryLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Site(ColumnMapping mapping)
        {
            var extracter = new TDSiteSummaryExtracter(mapping, dateOverride: StatsDate, streamReader: StreamReader, csvFilePath: FilePath);
            var loader = new TDSiteSummaryLoader(AccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        public void DoETL_Conv()
        {
            var platCode = GetAccount(AccountId).Platform.Code;
            var extracter = new TDConvExtracter(csvFilePath: FilePath, streamReader: StreamReader, platCode: platCode);
            var loader = new TDConvLoader(AccountId, platCode);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }


        public static ExtAccount GetAccount(int acctId)
        {
            using (var db = new ClientPortalProgContext())
            {
                return db.ExtAccounts
                    .Include(a => a.Platform.PlatColMapping)
                    .SingleOrDefault(a => a.Id == acctId);
            }
        }
    }

    public class StatsTypeAgg
    {
        public bool Daily { get; set; }
        public bool Strategy { get; set; }
        public bool AdSet { get; set; }
        public bool Creative { get; set; }
        public bool Site { get; set; }
        public bool Conv { get; set; }

        public bool All
        {
            get { return Daily && Strategy && AdSet && Creative && Site && Conv; }
        }

        public void SetAllTrue()
        {
            Daily = true;
            Strategy = true;
            AdSet = true;
            Creative = true;
            Site = true;
            Conv = true;
        }

        public StatsTypeAgg(string statsTypeString)
        {
            string statsTypeUpper = (statsTypeString == null) ? "" : statsTypeString.ToUpper();
            if (string.IsNullOrWhiteSpace(statsTypeUpper) || statsTypeUpper == "ALL")
                SetAllTrue();
            else if (statsTypeUpper.StartsWith("DAILY"))
                Daily = true;
            else if (statsTypeUpper.StartsWith("STRAT"))
                Strategy = true;
            else if (statsTypeUpper.StartsWith("ADSET"))
                AdSet = true;
            else if (statsTypeUpper.StartsWith("CREAT"))
                Creative = true;
            else if (statsTypeUpper.StartsWith("SITE"))
                Site = true;
            else if (statsTypeUpper.StartsWith("CONV"))
                Conv = true;
        }
    }
}
