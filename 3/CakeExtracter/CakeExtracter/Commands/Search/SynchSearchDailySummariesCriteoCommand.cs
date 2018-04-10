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
    public class SynchSearchDailySummariesCriteoCommand : ConsoleCommand
    {
        private const string criteoChannel = "Criteo";

        public int? SearchProfileId { get; set; }
        public string AccountCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TimeZoneOffset { get; set; }

        public override void ResetProperties()
        {
            SearchProfileId = null;
            AccountCode = null;
            StartDate = null;
            EndDate = null;
            TimeZoneOffset = 0;
        }

        public SynchSearchDailySummariesCriteoCommand()
        {
            IsCommand("synchSearchDailySummariesCriteo", "synch SearchDailySummaries for Criteo");
            HasOption<int>("p|searchProfileId=", "SearchProfile Id (default = all)", c => SearchProfileId = c);
            HasOption<string>("v|accountCode=", "Account Code", c => AccountCode = c);
            HasOption<DateTime>("s|startDate=", "Start Date (default is one month ago if no timeZoneOffet; otherwise 6 days ago)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            HasOption<int>("t|timeZoneOffset=", "TimeZoneOffset from GMT for stats (default = 0)", c => TimeZoneOffset = c); // -8 for Pacific Time
        }

        public override int Execute(string[] remainingArguments)
        {
            if (TimeZoneOffset != 0)
            {
                ExecuteHourly();
            }
            else // Limited to a 90-day window
            {
                var oneMonthAgo = DateTime.Today.AddMonths(-1);
                var yesterday = DateTime.Today.AddDays(-1);
                var dateRange = new DateRange(StartDate ?? oneMonthAgo, EndDate ?? yesterday);
                Logger.Info("Criteo ETL. DateRange {0}.", dateRange);

                foreach (var searchAccount in GetSearchAccounts())
                {
                    var extracter = new CriteoApiExtracter(searchAccount.AccountCode, dateRange);
                    var loader = new CriteoDailySummaryLoader(searchAccount.SearchAccountId);

                    loader.AddUpdateSearchCampaigns(extracter.GetCampaigns());

                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
            }
            return 0;
        }

        // Limited to a 7-day window and can go back to about 2 weeks from yesterday
        public void ExecuteHourly()
        {
            var sixDaysAgo = DateTime.Today.AddDays(-6);
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? sixDaysAgo, EndDate ?? yesterday);
            Logger.Info("Criteo ETL - hourly. DateRange {0}.", dateRange);

            foreach (var searchAccount in GetSearchAccounts())
            {
                var extracter = new CriteoApiExtracter2(searchAccount.AccountCode, dateRange, TimeZoneOffset);
                var loader = new CriteoDailySummaryLoader2(searchAccount.SearchAccountId);

                loader.AddUpdateSearchCampaigns(extracter.GetCampaigns());

                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
        }

        public IEnumerable<SearchAccount> GetSearchAccounts()
        {
            var searchAccounts = new List<SearchAccount>();

            using (var db = new ClientPortalContext())
            {
                if (this.AccountCode == null) // AccountCode not specified
                {
                    // Start with all criteo SearchAccounts with an account code
                    var searchAccountsQ = db.SearchAccounts.Where(sa => sa.Channel == criteoChannel && !String.IsNullOrEmpty(sa.AccountCode));
                    if (this.SearchProfileId.HasValue)
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId == this.SearchProfileId.Value); // ...for the specified SearchProfile
                    else
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId.HasValue); // ...that are children of a SearchProfile

                    searchAccounts = searchAccountsQ.ToList();
                }
                else // AccountCode specified
                {
                    var searchAccount = db.SearchAccounts.SingleOrDefault(sa => sa.AccountCode == AccountCode && sa.Channel == criteoChannel);
                    if (searchAccount != null)
                    {
                        if (SearchProfileId.HasValue && searchAccount.SearchProfileId != SearchProfileId.Value)
                            Logger.Warn("SearchProfileId does not match that of SearchAccount specified by AccountCode");

                        searchAccounts.Add(searchAccount);
                    }
                    else // didn't find a matching SearchAccount; see about creating a new one
                    {
                        SearchProfile searchProfile = null;
                        if (SearchProfileId.HasValue)
                            searchProfile = db.SearchProfiles.Find(SearchProfileId.Value);
                        if (searchProfile != null)
                        {
                            searchAccount = new SearchAccount()
                            {
                                SearchProfileId = this.SearchProfileId.Value,
                                Name = criteoChannel + " " + searchProfile.SearchProfileName,
                                Channel = criteoChannel,
                                AccountCode = AccountCode,
                            };
                            db.SearchAccounts.Add(searchAccount);
                            db.SaveChanges();
                            searchAccounts.Add(searchAccount);
                        }
                        else
                        {
                            Logger.Info("SearchAccount with AccountCode {0} not found and no valid SearchProfileId specified", AccountCode);
                        }
                    }
                }
            }
            return searchAccounts;
        }
    }
}
