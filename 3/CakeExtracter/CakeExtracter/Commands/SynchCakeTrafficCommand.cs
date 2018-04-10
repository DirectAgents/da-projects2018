using System;
using System.ComponentModel.Composition;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchCakeTrafficCommand : ConsoleCommand
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            StartDate = null;
            EndDate = null;
        }

        public SynchCakeTrafficCommand()
        {
            IsCommand("synchCakeTraffic", "synch cake traffic");
            HasOption("s|startDate=", "Start Date (default is one month ago)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is now)", c => EndDate = DateTime.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1);
            var dateRange = new DateRange(StartDate ?? oneMonthAgo, EndDate ?? now);

            var extracter = new TrafficExtracter(dateRange);
            var loader = new TrafficLoader();

            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            return 0;
        }

    }
}
