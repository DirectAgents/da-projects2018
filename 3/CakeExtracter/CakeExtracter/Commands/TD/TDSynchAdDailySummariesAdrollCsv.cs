using System;
using System.ComponentModel.Composition;
using System.IO;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class TDSynchAdDailySummariesAdrollCsv : ConsoleCommand
    {
        public static int RunStatic(StreamReader streamReader, int adrollProfileId, out string status)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new TDSynchAdDailySummariesAdrollCsv
            {
                StreamReader = streamReader,
                AdRollProfileId = adrollProfileId
            };
            int result = cmd.Run();
            status = cmd.Status;
            return result;
        }

        public StreamReader StreamReader { get; set; }

        public string CsvFilePath { get; set; }
        public int AdRollProfileId { get; set; }

        public override void ResetProperties()
        {
            CsvFilePath = null;
            AdRollProfileId = -1;
        }

        public TDSynchAdDailySummariesAdrollCsv()
        {
            IsCommand("tdSynchAdDailySummariesAdrollCsv", "synch AdDailySummaries for AdRoll CSV Report");
            HasRequiredOption("f|csvFilePath=", "CSV filepath", c => CsvFilePath = c);
            HasRequiredOption<int>("p|adrollProfileId=", "AdRollProfile id", c => AdRollProfileId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new AdrollCsvExtracter(CsvFilePath, StreamReader);
            var loader = new AdrollAdDailySummaryLoader(AdRollProfileId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            Status = String.Format("MinDate: {0:d}\nMaxDate: {1:d}\nNumCreatives: {2}", loader.MinDate, loader.MaxDate, loader.AdsAffected.Count);
            return 0;
        }

        public string Status { get; set; }

    }
}
