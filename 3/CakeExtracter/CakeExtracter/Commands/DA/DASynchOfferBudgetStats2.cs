using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchOfferBudgetStats2Command : ConsoleCommand
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysAgoToStart { get; set; }
        public int DaysToInclude { get; set; }

        private BudgetMonitor budgetMonitor;

        public override void ResetProperties()
        {
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = 0;
            DaysToInclude = 0;
        }

        public DASynchOfferBudgetStats2Command()
        {
            IsCommand("daSynchOfferBudgetStats2", "synch OfferDailySummaries with one Cake API call per day");
            HasOption("s|startDate=", "Start Date (default is today)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is startDate)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 0, i.e. today)", c => DaysAgoToStart = c);
            HasOption<int>("i|daysToInclude=", "Days to include, if endDate not specified (default = 1)", c => DaysToInclude = c);
            //AdvertiserId, OfferId
        }

        public override int Execute(string[] remainingArguments)
        {
            // 1) Get budgeted offers, loaded with current stats
            var offers = GetBudgetedOffers();

            // 2) Update DailySummaries
            if (DaysToInclude < 1) DaysToInclude = 1; // used if EndDate==null
            DateTime from = StartDate ?? DateTime.Today.AddDays(-DaysAgoToStart); // default: today
            DateTime to = EndDate ?? from.AddDays(DaysToInclude - 1); // default: whatever from is
            var dateRange = new DateRange(from, to);

            ExtractAndLoadOfferSummaries(dateRange);

            // 3) Update budgeted offer stats and send out alerts
            budgetMonitor = new BudgetMonitor();
            budgetMonitor.CheckOfferBudgetAlerts(offers);

            return 0;
        }
        public static void ExtractAndLoadOfferSummaries(DateRange dateRange)
        {
            foreach (var date in dateRange.Dates)
            {
                var extracter = new OfferSummaryExtracter(date);
                var loader = new DAOfferSummaryLoader(date);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
        }

        private IEnumerable<Offer> GetBudgetedOffers()
        {
            //TODO: Only get the offers with budgets whose dateRanges overlap with the above dateRange

            using (var repo = new DirectAgents.Domain.Concrete.MainRepository(new DAContext()))
            {
                var offers = repo.GetOffers(includeExtended: true, withBudget: true, includeInactive: false);
                foreach (var offer in offers)
                {
                    repo.FillOfferBudgetStats(offer);
                }
                return offers.ToList();
            }
        }
    }
}
