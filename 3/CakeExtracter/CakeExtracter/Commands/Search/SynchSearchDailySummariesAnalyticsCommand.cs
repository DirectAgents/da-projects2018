using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using CakeExtracter.Etl.SearchMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchSearchDailySummariesAnalyticsCommand : ConsoleCommand
    {
        public int AdvertiserId { get; set; }
        public string ClientProfileId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            ClientProfileId = null;
            StartDate = null;
            EndDate = null;
        }

        public SynchSearchDailySummariesAnalyticsCommand()
        {
            IsCommand("synchSearchDailySummariesAnalytics", "synch SearchDailySummaries for Google Analytics");
            HasOption<int>("a|advertiserId=", "Advertiser Id (default is 0, meaning all search advertisers, unless Client Customer Id specified)", c => AdvertiserId = c);
            HasOption<string>("v|clientProfileId=", "Client Profile Id", c => ClientProfileId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (default is one month ago)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var oneMonthAgo = DateTime.Today.AddMonths(-1);
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? oneMonthAgo, EndDate ?? yesterday);

            foreach (var advertiser in GetAdvertisers())
            {
                var extracter = new AnalyticsApiExtracter(advertiser.AnalyticsProfileId, dateRange);
                var loader = new AnalyticsApiLoader(advertiser.AdvertiserId);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
            return 0;
        }

        private IEnumerable<Advertiser> GetAdvertisers()
        {
            List<Advertiser> advertisers = new List<Advertiser>();
            using (var db = new ClientPortalContext())
            {
                if (this.AdvertiserId != 0) // get specified advertiser
                {
                    var adv = db.Advertisers.Where(a => a.AdvertiserId == AdvertiserId).FirstOrDefault();
                    if (adv == null)
                        Logger.Warn("Could not find advertiser with advertiserId {0}", AdvertiserId);
                    else if (String.IsNullOrWhiteSpace(adv.AnalyticsProfileId))
                        Logger.Warn("AnalyticsProfileId is not set for advertiserId {0}", AdvertiserId);
                    else
                        advertisers.Add(adv);
                }
                else if (this.ClientProfileId == null) // get all advertisers with a GA profile id
                {
                    var advs = db.Advertisers.Where(a => a.AnalyticsProfileId != null);
                    advertisers.AddRange(advs);
                }

                if (this.ClientProfileId != null) // get advertiser with specified GA profile id
                {
                    var advertiser = db.Advertisers.Where(a => a.AnalyticsProfileId == ClientProfileId).FirstOrDefault();
                    if (advertiser == null)
                        Logger.Warn("Could not find advertiser with AnalyticsProfileId {0}", ClientProfileId);
                    else
                        advertisers.Add(advertiser);
                }
            }
            return advertisers;
        }
    }
}
