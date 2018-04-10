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
    public class SynchSearchDailySummariesAdWordsCommand : ConsoleCommand
    {
        public static int RunStatic(int? searchProfileId = null, string clientId = null, DateTime? start = null, DateTime? end = null, int? daysAgoToStart = null, bool getClickAssistConvStats = false, bool getConversionTypeStats = false, bool getAllStats = false)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchSearchDailySummariesAdWordsCommand
            {
                SearchProfileId = searchProfileId,
                ClientId = clientId,
                StartDate = start,
                EndDate = end,
                DaysAgoToStart = daysAgoToStart,
                GetAllStats = getAllStats ? "true" : null,
                GetClickAssistConvStats = getClickAssistConvStats,
                GetConversionTypeStats = getConversionTypeStats
            };
            return cmd.Run();
        }

        public int? SearchProfileId { get; set; }
        public string ClientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }
        public bool IncludeClickType { get; set; }
        public int? MinSearchAccountId { get; set; }

        //TODO: make GetAllStats be bool. (change scheduled task on gogrid to use g=true)
        public string GetAllStats { get; set; } // "true" overrides the other get-stats properties
        public bool GetClickAssistConvStats { get; set; }
        public bool GetConversionTypeStats { get; set; }

        // If all are false, will get standard stats
        public bool GetStandardStats
        {
            get { return !GetClickAssistConvStats && !GetConversionTypeStats; }
        }

        public override void ResetProperties()
        {
            SearchProfileId = null;
            ClientId = null;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
            IncludeClickType = false;
            MinSearchAccountId = null;

            GetAllStats = null;
            GetClickAssistConvStats = false;
            GetConversionTypeStats = false;
        }

        public SynchSearchDailySummariesAdWordsCommand()
        {
            IsCommand("synchSearchDailySummariesAdWords", "synch SearchDailySummaries for AdWords");
            HasOption<int>("p|searchProfileId=", "SearchProfile Id (default = all)", c => SearchProfileId = c);
            HasOption<string>("v|clientId=", "Client Id", c => ClientId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (optional)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 41)", c => DaysAgoToStart = c);
            HasOption<bool>("b|includeClickType=", "Include ClickType (default is false)", c => IncludeClickType = c);
            HasOption<int>("m|minSearchAccountId=", "Include this and all higher searchAccountIds (optional)", c => MinSearchAccountId = c);

            HasOption<string>("g|getAllStats=", "Get all types of stats ('true', 'yes' or 'both', default is false)", c => GetAllStats = c);
            HasOption<bool>("k|getClickAssistConvStats=", "Get click-assisted-conversion stats (default is false)", c => GetClickAssistConvStats = c);
            HasOption<bool>("n|getConversionTypeStats=", "Get conversion-type stats (default is false)", c => GetConversionTypeStats = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            if (!DaysAgoToStart.HasValue)
                DaysAgoToStart = 41; // used if StartDate==null
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
            Logger.Info("AdWords ETL. DateRange {0}.", dateRange);

            var searchAccounts = GetSearchAccounts();
            foreach (var searchAccount in searchAccounts)
            {
                DateTime startDate = dateRange.FromDate;
                if (searchAccount.MinSynchDate.HasValue && (startDate < searchAccount.MinSynchDate.Value))
                    startDate = searchAccount.MinSynchDate.Value;
                var revisedDateRange = new DateRange(startDate, dateRange.ToDate);

                bool getAll = (GetAllStats == "true" || GetAllStats == "yes" || GetAllStats == "both");
                if (GetStandardStats || getAll)
                {
                    var extracter = new AdWordsApiExtracter(searchAccount.AccountCode, revisedDateRange, IncludeClickType);
                    var loader = new AdWordsApiLoader(searchAccount.SearchAccountId, IncludeClickType);
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
                if (GetClickAssistConvStats || getAll)
                {
                    var extracter = new AdWordsApiExtracter(searchAccount.AccountCode, revisedDateRange, IncludeClickType, clickAssistConvStats: true);
                    var loader = new AdWordsApiLoader(searchAccount.SearchAccountId, IncludeClickType, clickAssistConvStats: true);
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
                if (GetConversionTypeStats || getAll)
                {
                    // Note: IncludeClickType==true is not implemented
                    var extracter = new AdWordsApiExtracter(searchAccount.AccountCode, revisedDateRange, IncludeClickType, conversionTypeStats: true);
                    var loader = new AdWordsConvSummaryLoader(searchAccount.SearchAccountId);
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
            }
            return 0;
        }

        public IEnumerable<SearchAccount> GetSearchAccounts()
        {
            return GetSearchAccounts("Google", this.SearchProfileId, this.ClientId, minAccountId: this.MinSearchAccountId);
        }
        public static IEnumerable<SearchAccount> GetSearchAccounts(string channelName, int? searchProfileId, string accountCode, int? minAccountId = null)
        {
            var searchAccounts = new List<SearchAccount>();

            using (var db = new ClientPortalContext())
            {
                if (accountCode == null) // AccountCode not specified
                {
                    // Start with all channel SearchAccounts with an account code
                    var searchAccountsQ = db.SearchAccounts.Where(sa => sa.Channel == channelName && !String.IsNullOrEmpty(sa.AccountCode));
                    if (searchProfileId.HasValue)
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId == searchProfileId.Value); // ...for the specified SearchProfile
                    else
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchProfileId.HasValue); // ...that are children of a SearchProfile

                    if (minAccountId.HasValue)
                        searchAccountsQ = searchAccountsQ.Where(sa => sa.SearchAccountId >= minAccountId.Value);

                    searchAccounts = searchAccountsQ.OrderBy(sa => sa.SearchAccountId).ToList();
                    //searchAccounts = searchAccountsQ.OrderBy(sa => sa.SearchProfileId).ThenBy(sa => sa.SearchAccountId).ToList();
                }
                else // AccountCode specified
                {
                    var searchAccount = db.SearchAccounts.SingleOrDefault(sa => sa.AccountCode == accountCode && sa.Channel == channelName);
                    if (searchAccount != null)
                    {
                        if (searchProfileId.HasValue && searchAccount.SearchProfileId != searchProfileId.Value)
                            Logger.Warn("SearchProfileId does not match that of SearchAccount specified by AccountCode");

                        searchAccounts.Add(searchAccount);
                    }
                    else // didn't find a matching SearchAccount; see about creating a new one
                    {
                        if (searchProfileId.HasValue)
                        {
                            var searchProfile = db.SearchProfiles.Find(searchProfileId.Value);
                            if (searchProfile != null)
                            {
                                searchAccount = new SearchAccount()
                                {
                                    SearchProfile = searchProfile,
                                    Channel = channelName,
                                    AccountCode = accountCode
                                    // to fill in later: Name, ExternalId
                                };
                                db.SearchAccounts.Add(searchAccount);
                                db.SaveChanges();
                                searchAccounts.Add(searchAccount);
                            }
                            else
                            {
                                Logger.Info("SearchAccount with AccountCode {0} not found and SearchProfileId {1} not found", accountCode, searchProfileId);
                            }
                        }
                        else
                        {
                            Logger.Info("SearchAccount with AccountCode {0} not found and no SearchProfileId specified", accountCode);
                        }
                    }
                }
            }
            return searchAccounts;
        }

    }
}
