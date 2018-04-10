using System;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchDailySummariesCommand : ConsoleCommand
    {
        public int AdvertiserId { get; set; }
        public int? OfferId { get; set; } //note: if OfferId is null, then CheckCakeTraffic determines how to get the offerIds
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CheckCakeTraffic { get; set; }
        public bool IncludeDeletions { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            OfferId = null;
            StartDate = null;
            EndDate = null;
            CheckCakeTraffic = false;
            IncludeDeletions = false;
        }

        public SynchDailySummariesCommand()
        {
            //Note: These should have been run recently...
            // RunBefore(new SynchAdvertisersCommand());
            // RunBefore(new SynchOffersCommand());
            //TODO: a way to specify whether you want to run these or not?

            // Note: If CheckCakeTraffic is false, we rely on each offer's campaigns to be saved in the database
            //       (so synchCampaigns should have been run recently in that case)
            //       RunBefore(new SynchCampaignsCommand()); ...in some cases

            IsCommand("synchDailySummaries", "synch DailySummaries for an advertisers offers in a date range");
            HasOption<int>("a|advertiserId=", "Advertiser Id (default = 0 / all advertisers)", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
            HasOption("s|startDate=", "Start Date (default is 30 days ago)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is today)", c => EndDate = DateTime.Parse(c));
            HasOption("c|checkCakeTraffic=", "Check Cake to see which campaigns to update, rather than relying on the database (default = false)", c => CheckCakeTraffic = bool.Parse(c));
            HasOption("d|includeDeletions=", "clear dates with no data (default is false)", c => IncludeDeletions = bool.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var defaultStart = DateTime.Today.AddDays(-30);
            var dateRange = new DateRange(StartDate ?? defaultStart, EndDate ?? DateTime.Today);

            if (CheckCakeTraffic)
            {
                var span = (dateRange.ToDate - dateRange.FromDate);
                if (span.Days > 30)
                    Logger.Warn("Cake's OfferSummary and CampaignSummary reports require a date range of 31 days or less");
            }
            dateRange.ToDate = dateRange.ToDate.AddDays(1); // cake requires the date _after_ the last date you want stats for

            var startTime = DateTime.Now;

            var extracter = new DailySummariesExtracter(dateRange, AdvertiserId, OfferId, CheckCakeTraffic, IncludeDeletions);
            var loader = new DailySummariesLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            // Set "LatestDaySums" datetime (if we're updating all offers to today)
            if (dateRange.ToDate > DateTime.Today && !OfferId.HasValue)
                SetUpdateTimeForAdvertisers(startTime, AdvertiserId);

            return 0;
        }

        public static void SetUpdateTimeForAdvertisers(DateTime updateTime, int advertiserId = 0)
        {
            using (var db = new ClientPortalContext())
            {
                var advertisers = db.Advertisers.AsQueryable();
                if (advertiserId != 0)
                    advertisers = advertisers.Where(a => a.AdvertiserId == advertiserId);

                foreach (var advertiser in advertisers)
                {
                    advertiser.LatestDaySums = updateTime;
                }
                db.SaveChanges();
            }
        }

    }
}