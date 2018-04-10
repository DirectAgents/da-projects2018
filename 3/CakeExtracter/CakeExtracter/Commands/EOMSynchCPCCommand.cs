using System;
using System.ComponentModel.Composition;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using EomTool.Domain.Abstract;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class EOMSynchCPCCommand : ConsoleCommand
    {

        public static int RunStatic(int offerid,IMainRepository mainRepository,DateTime? start, DateTime? end)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new EOMSynchCPCCommand
                {
                    OfferId = offerid,
                    mainRepo = mainRepository,
                    StartDate = start,
                    EndDate = end
                };

            return cmd.Run();
        }

        public int OfferId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysAgoToStart { get; set; }
        public int DaysToInclude { get; set; }
        public IMainRepository mainRepo { get; set; }

        public override void ResetProperties()
        {
            OfferId = 0;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = 0;
            DaysToInclude = 0;
        }

        public EOMSynchCPCCommand()
        {
            IsCommand("EOMSynchCPC", "synch DailySummaries with one Cake API call per day");
            HasOption<int>("o|offerId=", "Offer Id (default = 0 / all offers)", c => OfferId = c);
            HasOption("s|startDate=", "Start Date (default is today)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is startDate)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 0, i.e. today)", c => DaysAgoToStart = c);
            HasOption<int>("i|daysToInclude=", "Days to include, if endDate not specified (default = 1)", c => DaysToInclude = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            //TODO: handle the case when the campaign hasn't been created; create it?
            //      can we do more than one day at a time?

            if (DaysToInclude < 1) DaysToInclude = 1; // used if EndDate==null
            DateTime from = StartDate ?? DateTime.Today.AddDays(-DaysAgoToStart); // default: today
            DateTime to = EndDate ?? from.AddDays(DaysToInclude - 1); // default: whatever from is
            var dateRange = new DateRange(from, to);
            int synchedCount = 0;
            foreach (var date in dateRange.Dates)
            {

                //var existingDailySummaries = DailySummaries(date);
                //var initialOffAffs = GetOffAffs(date);

                var extracter = new CPCExtracter(date, offerId: OfferId);

                var loader = new CPCLoader(date, mainRepo);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
                synchedCount += loader.LoadedCount;
            }

            return synchedCount;
        }
        
    }
}
