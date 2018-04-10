using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using ClientPortal.Data.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchOfferDailySummariesCommand : ConsoleCommand
    {
        public string Advertiser { get; set; }
        public int? OfferId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            Advertiser = null;
            OfferId = null;
            StartDate = null;
            EndDate = null;
        }

        public SynchOfferDailySummariesCommand()
        {
            //RunBefore(new SynchAdvertisersCommand());
            //RunBefore(new SynchOffersCommand());

            IsCommand("synchOfferDailySummaries", "synch OfferDailySummaries for an advertisers offers in a date range");
            HasOption("a|advertiserId=", "Advertiser Id (* = all advertisers)", c => Advertiser = c);
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
            HasOption("s|startDate=", "Start Date (default is two months ago)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is today)", c => EndDate = DateTime.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var twoMonthAgo = DateTime.Today.AddMonths(-2);
            var dateRange = new DateRange(StartDate ?? twoMonthAgo, EndDate ?? DateTime.Today);

            dateRange.ToDate = dateRange.ToDate.AddDays(1); // cake requires the date _after_ the last date you want stats for

            var advertiserIds = GetAdvertiserIds();
            var extracter = new OfferDailySummariesExtracter(dateRange, advertiserIds, OfferId, false);
            var loader = new OfferDailySummariesLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            // Set "LatestDaySums" datetime (if we're updating all offers to today)
            if (dateRange.ToDate > DateTime.Today && !OfferId.HasValue)
            {
                using (var db = new ClientPortalContext())
                {
                    var advertisers = db.Advertisers.Where(a => advertiserIds.Contains(a.AdvertiserId));
                    foreach (var advertiser in advertisers)
                    {
                        advertiser.LatestDaySums = DateTime.Now;
                    }
                    db.SaveChanges();
                }
            }
            return 0;
        }

        private List<int> GetAdvertiserIds()
        {
            List<int> advertiserIds = null;
            if (string.IsNullOrWhiteSpace(Advertiser) || Advertiser == "*")
            {
                using (var db = new ClientPortalContext())
                {
                    if (OfferId.HasValue)
                    {
                        var offer = db.Offers.FirstOrDefault(o => o.OfferId == OfferId.Value);
                        if (offer != null && offer.AdvertiserId.HasValue)
                            advertiserIds = new List<int> { offer.AdvertiserId.Value };
                    }
                    else
                    {
                        advertiserIds = db.Advertisers
                                          .Where(c => c.AdvertiserId < 90000)
                                          .OrderBy(c => c.AdvertiserId)
                                          .Select(c => c.AdvertiserId)
                                          .ToList();
                    }
                }
            }
            else
            {
                advertiserIds = new List<int> { int.Parse(Advertiser) };
            }
            return advertiserIds;
        }

    }
}