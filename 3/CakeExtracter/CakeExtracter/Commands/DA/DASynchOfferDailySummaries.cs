using System;
using System.ComponentModel.Composition;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchOfferDailySummaries : ConsoleCommand
    {
        public static int RunStatic(int advertiserId, int? offerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchOfferDailySummaries
            {
                AdvertiserId = advertiserId,
                OfferId = offerId,
                StartDate = startDate,
                EndDate = endDate
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }
        public int? OfferId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            OfferId = null;
            StartDate = null;
            EndDate = null;
        }

        public DASynchOfferDailySummaries()
        {
            IsCommand("daSynchOfferDailySummaries", "synch OfferDailySummaries for an advertisers offers in a date range");
            HasOption<int>("a|advertiserId=", "Advertiser Id (default = 0 / all advertisers)", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
            HasOption("s|startDate=", "Start Date (default is two months ago)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is today)", c => EndDate = DateTime.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var twoMonthAgo = DateTime.Today.AddMonths(-2);
            var dateRange = new DateRange(StartDate ?? twoMonthAgo, EndDate ?? DateTime.Today);

            dateRange.ToDate = dateRange.ToDate.AddDays(1); // cake requires the date _after_ the last date you want stats for

            var extracter = new OfferDailySummariesExtracter(dateRange, AdvertiserId, OfferId, false);
            var loader = new DAOfferDailySummariesLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }
    }
}
