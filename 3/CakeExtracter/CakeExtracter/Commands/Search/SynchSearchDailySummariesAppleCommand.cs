using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Apple;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using CakeExtracter.Etl.SearchMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchSearchDailySummariesAppleCommand : ConsoleCommand
    {
        public static int RunStatic(int? searchProfileId = null, string clientId = null, DateTime? start = null, DateTime? end = null, int? daysAgoToStart = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchSearchDailySummariesAppleCommand
            {
                SearchProfileId = searchProfileId,
                ClientId = clientId,
                StartDate = start,
                EndDate = end,
                DaysAgoToStart = daysAgoToStart
            };
            return cmd.Run();
        }

        public int? SearchProfileId { get; set; }
        public string ClientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }

        public override void ResetProperties()
        {
            SearchProfileId = null;
            ClientId = null;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
        }

        public SynchSearchDailySummariesAppleCommand()
        {
            IsCommand("synchSearchDailySummariesApple", "synch SearchDailySummaries for Apple");
            HasOption<int>("p|searchProfileId=", "SearchProfile Id (default = all)", c => SearchProfileId = c);
            HasOption<string>("v|clientId=", "Client Id", c => ClientId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (optional)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 41)", c => DaysAgoToStart = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            try
            {
                if (!DaysAgoToStart.HasValue)
                    DaysAgoToStart = 41; // used if StartDate==null
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
                Logger.Info("Apple ETL. DateRange {0}.", dateRange);

                var appleAdsUtility = new AppleAdsUtility(m => Logger.Info(m), m => Logger.Warn(m));
                var searchAccounts = GetSearchAccounts();
                Logger.Info("Returned from GetSearchAccounts().");

                foreach (var searchAccount in searchAccounts)
                {
                    DateTime startDate = dateRange.FromDate;
                    if (searchAccount.MinSynchDate.HasValue && (startDate < searchAccount.MinSynchDate.Value))
                        startDate = searchAccount.MinSynchDate.Value;
                    var revisedDateRange = new DateRange(startDate, dateRange.ToDate);

                    var extracter = new AppleApiExtracter(appleAdsUtility, revisedDateRange, searchAccount.AccountCode, searchAccount.ExternalId);
                    var loader = new AppleApiLoader(searchAccount.SearchAccountId);
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return 0;
        }

        public IEnumerable<SearchAccount> GetSearchAccounts()
        {
            return SynchSearchDailySummariesAdWordsCommand.GetSearchAccounts("Apple", this.SearchProfileId, this.ClientId);
        }
    }

}
