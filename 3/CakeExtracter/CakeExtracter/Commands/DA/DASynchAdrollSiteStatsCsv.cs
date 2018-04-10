using System;
using System.ComponentModel.Composition;
using System.IO;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchAdrollSiteStatsCsv : ConsoleCommand
    {
        public StreamReader StreamReader { get; set; }

        public string CsvFilePath { get; set; }
        //public int AdRollProfileId { get; set; }
        public int AccountId { get; set; }
        public DateTime date { get; set; }

        public override void ResetProperties()
        {
            CsvFilePath = null;
            //AdRollProfileId = -1;
            AccountId = -1;
        }

        //TODO: account id / advertiseable id

        public DASynchAdrollSiteStatsCsv()
        {
            IsCommand("daSynchAdrollSiteStatsCsv", "synch SiteStats from AdRoll CSV Report");
            HasRequiredOption("f|csvFilePath=", "CSV filepath", c => CsvFilePath = c);
            //HasRequiredOption<int>("p|adrollProfileId=", "AdRollProfile id", c => AdRollProfileId = c);
            HasRequiredOption<int>("a|accountId=", "ExtAccount id", c => AccountId = c);
            HasRequiredOption<DateTime>("d|date=", "Date", c => date = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new AdrollSiteStatsCsvExtracter(CsvFilePath, StreamReader);
            var loader = new AdrollSiteStatsLoader(AccountId, date);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }
    }
}

    

