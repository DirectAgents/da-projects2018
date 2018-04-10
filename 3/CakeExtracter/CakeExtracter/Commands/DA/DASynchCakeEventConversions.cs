using System;
using System.ComponentModel.Composition;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchCakeEventConversions : ConsoleCommand
    {
        public int? AdvertiserId { get; set; }
        public int? OfferId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = null;
            OfferId = null;
            StartDate = null;
            EndDate = null;
        }

        public DASynchCakeEventConversions()
        {
            IsCommand("daSynchCakeEventConversions", "synch EventConversions");
            HasOption<int>("a|advertiserId=", "Advertiser Id (default = 0 / all advertisers)", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (default = 0 / all offers)", c => OfferId = c);
            HasOption("s|startDate=", "Start Date (default yesterday)", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is yesterday)", c => EndDate = DateTime.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var yesterday = DateTime.Today.AddDays(-1);
            DateTime from = StartDate ?? yesterday;
            DateTime to = EndDate ?? yesterday;
            var dateRange = new DateRange(from, to);

            var extracter = new EventConversionExtracter(dateRange, AdvertiserId ?? 0, OfferId ?? 0);
            var loader = new DAEventConversionLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }

    }
}
