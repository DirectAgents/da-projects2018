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
    public class SynchSearchCallDailySummariesCommand : ConsoleCommand
    {
        public int? SearchProfileId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            SearchProfileId = null;
            StartDate = null;
            EndDate = null;
        }

        public SynchSearchCallDailySummariesCommand()
        {
            IsCommand("synchSearchCallDailySummaries", "synch CallDailySummaries");
            HasOption<int>("p|searchProfileId=", "SearchProfile Id (default = all)", c => SearchProfileId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (default is 3 days ago)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var threeDaysAgo = DateTime.Today.AddDays(-3);
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? threeDaysAgo, EndDate ?? yesterday);
            Logger.Info("LocalConnex ETL. DateRange {0}.", dateRange);

            foreach (var searchProfile in GetSearchProfiles())
            {
                var extracter = new LocalConnexApiExtracter(dateRange, searchProfile.LCaccid, searchProfile.CallMinSeconds);
                var loader = new CallDailySummaryLoader(searchProfile.SearchProfileId);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
            return 0;
        }

        public IEnumerable<SearchProfile> GetSearchProfiles()
        {
            using (var db = new ClientPortalContext())
            {
                var searchProfiles = db.SearchProfiles.Include("SearchAccounts.SearchCampaigns").AsQueryable();

                if (this.SearchProfileId.HasValue)
                    searchProfiles = searchProfiles.Where(sp => sp.SearchProfileId == this.SearchProfileId.Value);
                else
                    searchProfiles = searchProfiles.Where(sp => sp.LCaccid != null && sp.LCaccid != "0");
                    // if profileId not specified, get all SearchProfiles that have a non-null, non-0 LCaccid

                return searchProfiles.ToList();
            }
        }

    }
}
