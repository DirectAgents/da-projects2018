using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using Yahoo;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchYAMStats : ConsoleCommand
    {
        public static int RunStatic(int? accountId = null, DateTime? startDate = null, DateTime? endDate = null, string statsType = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchYAMStats
            {
                AccountId = accountId,
                StartDate = startDate,
                EndDate = endDate,
                StatsType = statsType
            };
            return cmd.Run();
        }

        public int? AccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }
        public string StatsType { get; set; }
        public bool DisabledOnly { get; set; }

        private YAMUtility yamUtility { get; set; }
        private string[] extIds_UsePixelParm;

        public override void ResetProperties()
        {
            AccountId = null;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
            StatsType = null;
            DisabledOnly = false;
        }

        public DASynchYAMStats()
        {
            IsCommand("daSynchYAMStats", "synch YAM Stats");
            HasOption<int>("a|accountId=", "Account Id (default = all)", c => AccountId = c);
            HasOption("s|startDate=", "Start Date (default is 'daysAgo')", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is yesterday)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 31)", c => DaysAgoToStart = c);
            HasOption<string>("t|statsType=", "Stats Type (default: all)", c => StatsType = c);
            HasOption<bool>("x|disabledOnly=", "Include only disabled accounts (default = false)", c => DisabledOnly = c);
        }

        //TODO: if synching all accounts, can we make one API call to get everything?

        public override int Execute(string[] remainingArguments)
        {
            if (!DaysAgoToStart.HasValue)
                DaysAgoToStart = 31; // used if StartDate==null
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
            Logger.Info("YAM ETL. DateRange {0}.", dateRange);

            var statsType = new StatsTypeAgg(this.StatsType);
            SetupYAMUtility();
            var ppIds = ConfigurationManager.AppSettings["YAMids_UsePixelParm"];
            if (ppIds != null)
                extIds_UsePixelParm = ppIds.Split(new char[] { ',' });
            else
                extIds_UsePixelParm = new string[] { };

            var accounts = GetAccounts();
            foreach (var account in accounts)
            {
                Logger.Info("Commencing ETL for YAM account ({0}) {1}", account.Id, account.Name);
                yamUtility.SetWhichAlt(account.ExternalId);
                try
                {
                    if (statsType.Daily)
                        DoETL_Daily(dateRange, account);
                    if (statsType.Strategy)
                        DoETL_Strategy(dateRange, account);
                    if (statsType.Creative)
                        DoETL_Creative(dateRange, account);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            SaveTokens();
            return 0;
        }

        private void SetupYAMUtility()
        {
            this.yamUtility = new YAMUtility(m => Logger.Info(m), m => Logger.Warn(m));
            GetTokens();
        }
        private void GetTokens()
        {
            // Get tokens, if any, from the database
            string[] tokenSets = Platform.GetPlatformTokens(Platform.Code_YAM);
            yamUtility.TokenSets = tokenSets;
        }
        private void SaveTokens()
        {
            Platform.SavePlatformTokens(Platform.Code_YAM, yamUtility.TokenSets);
        }

        private void DoETL_Daily(DateRange dateRange, ExtAccount account)
        {
            var extracter = new YAMDailySummaryExtracter(yamUtility, dateRange, account);
            var loader = new TDDailySummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            if (extIds_UsePixelParm.Contains(account.ExternalId))
            {   // Get ConVals using the pixel parameter...
                var e = new YAMDailyConValExtracter(yamUtility, dateRange, account);
                var l = new TDDailyConValLoader(account.Id);
                var eThread = e.Start();
                var lThread = l.Start(e);
                eThread.Join();
                lThread.Join();
            }
        }
        private void DoETL_Strategy(DateRange dateRange, ExtAccount account)
        {
            var extracter = new YAMStrategySummaryExtracter(yamUtility, dateRange, account);
            var loader = new TDStrategySummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            if (extIds_UsePixelParm.Contains(account.ExternalId))
            {   // Get ConVals using the pixel parameter...
                string[] stratNames = GetExistingStrategyNames(dateRange, account.Id);
                var e = new YAMStrategyConValExtracter(yamUtility, dateRange, account, existingStrategyNames: stratNames);
                var l = new TDStrategyConValLoader(account.Id);
                var eThread = e.Start();
                var lThread = l.Start(e);
                eThread.Join();
                lThread.Join();
            }
        }
        private void DoETL_Creative(DateRange dateRange, ExtAccount account)
        {
            var extracter = new YAMTDadSummaryExtracter(yamUtility, dateRange, account);
            var loader = new TDadSummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

        //Get the names of strategies that have stats for the specified dateRange and account (and whose stats' ConVals are not zero)
        private string[] GetExistingStrategyNames(DateRange dateRange, int accountId)
        {
            using (var db = new ClientPortalProgContext())
            {
                var stratSums = db.StrategySummaries.Where(s => s.Strategy.AccountId == accountId && s.Date >= dateRange.FromDate && s.Date <= dateRange.ToDate
                                                           && (s.PostClickRev != 0 || s.PostViewRev != 0));
                var strategies = stratSums.Select(s => s.Strategy).Distinct();
                var stratNames = strategies.Select(s => s.Name).Distinct().ToArray();
                return stratNames;
            }
        }

        private IEnumerable<ExtAccount> GetAccounts()
        {
            using (var db = new ClientPortalProgContext())
            {
                var accounts = db.ExtAccounts.Include("Platform.PlatColMapping").Where(a => a.Platform.Code == Platform.Code_YAM);
                if (AccountId.HasValue)
                    accounts = accounts.Where(a => a.Id == AccountId.Value);
                else if (!DisabledOnly)
                    accounts = accounts.Where(a => !a.Disabled);

                if (DisabledOnly)
                    accounts = accounts.Where(a => a.Disabled);

                return accounts.ToList().Where(a => !string.IsNullOrWhiteSpace(a.ExternalId));
            }
        }

    }
}
