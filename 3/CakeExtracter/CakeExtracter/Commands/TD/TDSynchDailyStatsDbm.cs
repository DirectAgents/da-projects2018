using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class TDSynchDailyStatsDbm : ConsoleCommand
    {
        //public bool doLocationStats { get; set; }

        public override void ResetProperties()
        {
            //doLocationStats = false;
        }

        public TDSynchDailyStatsDbm()
        {
            IsCommand("tdSynchDailyStatsDbm", "synch various daily DBM stats");
        }

        public override int Execute(string[] remainingArguments)
        {
            string bucket_DailyLocationStats = "151075984680687222131433364265094_report"; //TODO: config
            DateTime date = DateTime.Today; // will get yesterday's stats

            var extracter = new DbmDailyStatsExtracter(bucket_DailyLocationStats, date);
            var loader = new DbmDailyStatsLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }
    }
}
