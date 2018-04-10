using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchClicksCommand : ConsoleCommand
    {
        public int AdvertiserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool SynchConversionsAlso { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            StartDate = null;
            EndDate = null;
            SynchConversionsAlso = false;
        }

        public SynchClicksCommand()
        {
            IsCommand("synchClicks", "synch Clicks for an advertisers offers in a date range");
            HasOption("a|advertiserId=", "Advertiser Id (0 = all)", c => AdvertiserId = int.Parse(c));
            HasOption("s|startDate=", "Start Date (default is two days ago)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is yesterday)", c => EndDate = DateTime.Parse(c));
            HasOption("c|conversions=", "synch Conversions also (default is false)", c => SynchConversionsAlso = bool.Parse(c));
            //TODO: # of days to go back (ignored if StartDate.HasValue)
            //TODO? Starting with yesterday, see if that day has any clicks/convs. keep going back if none are found (x days max).
        }

        public override int Execute(string[] remainingArguments)
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? yesterday.AddDays(-1), EndDate ?? yesterday);
            var advertiserIds = GetAdvertiserIds();

            foreach (var date in dateRange.Dates)
            {
                try
                {
                    ExtractAndLoadClicksForDate(date, advertiserIds); // When advId==0, use advIds from DB (UserProfile table, advIds<90000)
                    if (SynchConversionsAlso)
                        ExtractAndLoadConversionsForDate(date); // When advId==0, get all conversions from Cake

                    // Set "LatestClicks" datetime
                    using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
                    {
                        var now = DateTime.Now;
                        foreach (var advertiserId in advertiserIds)
                        {
                            var advertiser = db.Advertisers.Where(a => a.AdvertiserId == advertiserId).FirstOrDefault();
                            if (advertiser != null)
                                advertiser.LatestClicks = now;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                    Logger.Warn(ex.StackTrace);
                    Logger.Error(ex);
                }
            }
            return 0;
        }

        private static void ExtractAndLoadClicksForDate(DateTime date, IEnumerable<int> advertiserIds)
        {
            // Does one advertiser at a time...
            foreach (var advertiserId in advertiserIds)
            {
                Logger.Info("Extracting clicks for {0}..", date.ToShortDateString());
                var dateRange = new DateRange(date, date.AddDays(1));
                var extracter = new ClicksExtracter(dateRange, advertiserId);
                var loader = new ClicksLoader();
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
                Logger.Info("Finished extracting clicks for {0}.", date.ToShortDateString());
            }
        }

        private void ExtractAndLoadConversionsForDate(DateTime date)
        {
            // TODO: Handle "deleted" conversions.  ?Clear them out for the day, then reload?

            // Does all advertisers together (if AdvId 0 specified); just one if AdvId > 0
            Logger.Info("Extracting conversions for {0}..", date.ToShortDateString());
            var dateRange = new DateRange(date, date.AddDays(1));
            var extracter = new ConversionsExtracter(dateRange, AdvertiserId);
            var loader = new ConversionsLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            Logger.Info("Finished extracting conversions for {0}.", date.ToShortDateString());
        }

        private IEnumerable<int> GetAdvertiserIds()
        {
            if (AdvertiserId == 0)
            {
                using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
                {
                    var advertiserIds = db.UserProfiles
                                          .Where(c => c.CakeAdvertiserId.HasValue && c.CakeAdvertiserId > 0 && c.CakeAdvertiserId < 90000)
                                          .Select(c => c.CakeAdvertiserId.Value)
                                          .OrderBy(c => c)
                                          .ToList();
                    return advertiserIds;
                }
            }
            return new[] { AdvertiserId };
        }
    }
}
