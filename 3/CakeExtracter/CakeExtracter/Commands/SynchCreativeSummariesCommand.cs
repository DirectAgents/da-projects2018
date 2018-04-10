using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchCreativeSummariesCommand : ConsoleCommand
    {
        public static int RunStatic(DateTime? startDate, DateTime? endDate, int? offerId, int? creativeId)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchCreativeSummariesCommand
            {
                StartDate = startDate,
                EndDate = endDate,
                OfferId = offerId,
                CreativeId = creativeId
            };
            return cmd.Run();
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? OfferId { get; set; }
        public int? CreativeId { get; set; }

        public override void ResetProperties()
        {
            StartDate = null;
            EndDate = null;
            OfferId = null;
            CreativeId = null;
        }

        public SynchCreativeSummariesCommand()
        {
            IsCommand("synchCreativeSummaries", "synch CreativeSummaries");
            HasOption("s|startDate=", "Start Date (default is beginning of last month)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is today)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
            HasOption<int>("c|creativeId=", "Creative Id (default = all)", c => CreativeId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var oneMonthAgo = DateTime.Today.AddMonths(-1);
            var beginningOfLastMonth = new DateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
            var dateRange = new DateRange(StartDate ?? beginningOfLastMonth, EndDate ?? DateTime.Today);

            dateRange.ToDate = dateRange.ToDate.AddDays(1); // cake requires the date _after_ the last date you want stats for

            var creatives = GetCreatives();
            foreach (var creative in creatives)
            {
                var extracter = new CreativeMonthlySummariesExtracter(dateRange, creative.CreativeId);
                var loader = new CreativeMonthlySummariesLoader();
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
            return 0;
        }

        private IEnumerable<Creative> GetCreatives()
        {
            using (var db = new ClientPortalContext())
            {
                var creatives = db.Creatives.AsQueryable();
                if (OfferId.HasValue)
                    creatives = creatives.Where(c => c.OfferId == OfferId.Value);
                if (CreativeId.HasValue)
                    creatives = creatives.Where(c => c.CreativeId == CreativeId.Value);

                return creatives.ToList();
            }
        }
    }
}
