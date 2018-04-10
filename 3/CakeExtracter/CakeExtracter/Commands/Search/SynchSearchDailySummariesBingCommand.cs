using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using CakeExtracter.Etl.SearchMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchSearchDailySummariesBingCommand : ConsoleCommand
    {
        public static int RunStatic(int? searchProfileId = null, int? accountId = null, DateTime? start = null, DateTime? end = null, int? daysAgoToStart = null, bool getConversionTypeStats = false)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchSearchDailySummariesBingCommand
            {
                SearchProfileId = searchProfileId,
                AccountId = accountId ?? 0,
                StartDate = start,
                EndDate = end,
                DaysAgoToStart = daysAgoToStart,
                GetConversionTypeStats = getConversionTypeStats
            };
            return cmd.Run();
        }

        public int? SearchProfileId { get; set; }
        public int AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }

        private bool? _includeShopping;
        private bool? _includeNonShopping;
        public bool IncludeShopping
        {
            get { return (!_includeShopping.HasValue || _includeShopping.Value); }
        }   // default: true
        public bool IncludeNonShopping
        {
            get { return (!_includeNonShopping.HasValue || _includeNonShopping.Value); }
        }   // default: true

        public bool GetConversionTypeStats { get; set; }

        public override void ResetProperties()
        {
            SearchProfileId = null;
            AccountId = 0;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
            _includeShopping = null;
            _includeNonShopping = null;
            GetConversionTypeStats = false;
        }

        public SynchSearchDailySummariesBingCommand()
        {
            IsCommand("synchSearchDailySummariesBing", "synch SearchDailySummaries for Bing API Report");
            HasOption<int>("p|searchProfileId=", "SearchProfile Id (default = all)", c => SearchProfileId = c);
            HasOption<int>("v|accountId=", "Account Id", c => AccountId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (optional)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 41)", c => DaysAgoToStart = c);
            HasOption<bool>("r|includeRegular=", "Include Regular(NonShopping) campaigns (default is true)", c => _includeNonShopping = c);
            HasOption<bool>("h|includeShopping=", "Include Shopping campaigns (default is true)", c => _includeShopping = c);
            HasOption<bool>("n|getConversionTypeStats=", "Get conversion-type stats (default is false)", c => GetConversionTypeStats = c);
            //TODO? change to default:true ?
        }

        public override int Execute(string[] remainingArguments)
        {
            //GlobalProxySelection.Select = new WebProxy("127.0.0.1", 8888);
            if (!DaysAgoToStart.HasValue)
                DaysAgoToStart = 41; // used if StartDate==null
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
            Logger.Info("Bing ETL. DateRange {0}.", dateRange);

            foreach (var searchAccount in GetSearchAccounts())
            {
                DateTime startDate = dateRange.FromDate;
                DateTime endDate = dateRange.ToDate;
                if (searchAccount.MinSynchDate.HasValue && (startDate < searchAccount.MinSynchDate.Value))
                    startDate = searchAccount.MinSynchDate.Value;

                int accountId;
                if (int.TryParse(searchAccount.AccountCode, out accountId))
                {
                    if (IncludeShopping || IncludeNonShopping)
                    {
                        var extracter = new BingDailySummaryExtracter(accountId, startDate, endDate, includeShopping: IncludeShopping, includeNonShopping: IncludeNonShopping);
                        var loader = new BingLoader(searchAccount.SearchAccountId);
                        var extracterThread = extracter.Start();
                        var loaderThread = loader.Start(extracter);
                        extracterThread.Join();
                        loaderThread.Join();
                    }
                    if (GetConversionTypeStats)
                    {
                        //TODO: handle dates with no stats... keep track of all dates within the range and for those missing when done, delete the SCS's
                        //      (could do in extracter or loader or have loader return dates loaded, or missing dates, or have a method to call to delete SCS's
                        //       that didn't have any items)
                        var extracter = new BingConvSummaryExtracter(accountId, startDate, endDate);
                        var loader = new BingConvSummaryLoader(searchAccount.SearchAccountId);
                        var extracterThread = extracter.Start();
                        var loaderThread = loader.Start(extracter);
                        extracterThread.Join();
                        loaderThread.Join();
                    }
                }
                else
                    Logger.Info("AccountCode should be an int. Skipping: {0}", searchAccount.AccountCode);
            }
            return 0;
        }

        public IEnumerable<SearchAccount> GetSearchAccounts()
        {
            var searchAccounts = new List<SearchAccount>();

            using (var db = new ClientPortalContext())
            {
                if (this.AccountId == 0) // AccountId not specified
                {
                    // Start with all bing SearchAccounts with an account code
                    var searchAccountsQ = db.SearchAccounts.Where(sa => sa.Channel == "Bing" && !String.IsNullOrEmpty(sa.AccountCode));
                    if (this.SearchProfileId.HasValue)
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId == this.SearchProfileId.Value); // ...for the specified SearchProfile
                    else
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId.HasValue); // ...that are children of a SearchProfile

                    searchAccounts = searchAccountsQ.ToList();
                }
                else // AccountId specified
                {
                    var accountIdString = AccountId.ToString();
                    var searchAccount = db.SearchAccounts.SingleOrDefault(sa => sa.AccountCode == accountIdString && sa.Channel == "Bing");
                    if (searchAccount != null)
                    {
                        if (SearchProfileId.HasValue && searchAccount.SearchProfileId != SearchProfileId.Value)
                            Logger.Warn("SearchProfileId does not match that of SearchAccount specified by AccountId");

                        searchAccounts.Add(searchAccount);
                    }
                    else // didn't find a matching SearchAccount; see about creating a new one
                    {
                        if (SearchProfileId.HasValue)
                        {
                            searchAccount = new SearchAccount()
                            {
                                SearchProfileId = this.SearchProfileId.Value,
                                Channel = "Bing",
                                AccountCode = accountIdString
                                // to fill in later: Name, ExternalId
                            };
                            db.SearchAccounts.Add(searchAccount);
                            db.SaveChanges();
                            searchAccounts.Add(searchAccount);
                        }
                        else
                        {
                            Logger.Info("SearchAccount with AccountCode {0} not found and no SearchProfileId specified", AccountId);
                        }
                    }
                }
            }
            return searchAccounts;
        }

    }
}
