using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;
using System.ComponentModel.Composition;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class TDSynchCreativeDailySummariesDbmCsv : ConsoleCommand
    {
        public string CsvFile { get; set; }

        public override void ResetProperties()
        {
            CsvFile = null;
        }

        public TDSynchCreativeDailySummariesDbmCsv()
        {
            IsCommand("tdSynchCreativeDailySummariesDbmCsv", "synch CreativeDailySummaries for DBM CSV Report");
            HasRequiredOption("f|csvFile=", "CSV File", c => CsvFile = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new DbmCsvExtracter(CsvFile, true);
            var loader = new DbmCreativeDailySummaryLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            return 0;
        }

    }
}
