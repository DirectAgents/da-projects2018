using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.SocialMarketing.Extracters;
using CakeExtracter.Etl.SocialMarketing.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchFacebookStats : ConsoleCommand
    {
        public static int RunStatic(int? accountId = null, DateTime? startDate = null, DateTime? endDate = null, string statsType = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchFacebookStats
            {
                AccountId = accountId,
                StartDate = startDate,
                EndDate = endDate,
                StatsType = statsType
            };
            return cmd.Run();
        }

        public int? AccountId { get; set; }
        public int? CampaignId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }
        public string StatsType { get; set; }
        public bool DisabledOnly { get; set; }
        public int? DaysPerCall { get; set; }

        public override void ResetProperties()
        {
            AccountId = null;
            CampaignId = null;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
            StatsType = null;
            DisabledOnly = false;
            DaysPerCall = null;
        }

        public DASynchFacebookStats()
        {
            IsCommand("daSynchFacebookStats", "synch Facebook stats");
            HasOption<int>("a|accountId=", "Account Id (default = all)", c => AccountId = c);
            HasOption<int>("c|campaignId=", "Campaign Id (optional)", c => CampaignId = c);
            HasOption("s|startDate=", "Start Date (default is 'daysAgo')", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is yesterday)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 41)", c => DaysAgoToStart = c);
            HasOption<string>("t|statsType=", "Stats Type (default: all)", c => StatsType = c);
            HasOption<bool>("x|disabledOnly=", "Include only disabled accounts (default = false)", c => DisabledOnly = c);
            HasOption<int>("p|daysPerCall=", "Days Per API call (default: varies per stats type)", c => DaysPerCall = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            if (!DaysAgoToStart.HasValue)
                DaysAgoToStart = 41; // used if StartDate==null
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
            Logger.Info("Facebook ETL. DateRange {0}.", dateRange);

            var statsType = new StatsTypeAgg(this.StatsType);
            var fbUtility = new FacebookUtility(m => Logger.Info(m), m => Logger.Warn(m));
            fbUtility.DaysPerCall_Override = DaysPerCall;

            var string_ConvAsMobAppInst = ConfigurationManager.AppSettings["FB_ConversionsAsMobileAppInstalls"] ?? "";
            var Accts_ConvAsMobAppInst = string_ConvAsMobAppInst.Split(new char[] { ',' });
            var string_ConvAsPurch = ConfigurationManager.AppSettings["FB_ConversionsAsPurchases"] ?? "";
            var Accts_ConvAsPurch = string_ConvAsPurch.Split(new char[] { ',' });
            var string_ConvAsReg = ConfigurationManager.AppSettings["FB_ConversionsAsRegistrations"] ?? "";
            var Accts_ConvAsReg = string_ConvAsReg.Split(new char[] { ',' });
            var string_ConvAsVideoPlay = ConfigurationManager.AppSettings["FB_ConversionsAsVideoPlays"] ?? "";
            var Accts_ConvAsVideoPlay = string_ConvAsVideoPlay.Split(new char[] { ',' });

            var string_7d_click = ConfigurationManager.AppSettings["FB_7d_click"] ?? "";
            var Accts_7d_click = string_7d_click.Split(new char[] { ',' });
            var string_7d_view = ConfigurationManager.AppSettings["FB_7d_view"] ?? "";
            var Accts_7d_view = string_7d_view.Split(new char[] { ',' });

            var Accts_DailyOnly = new string[] { };
            if (!AccountId.HasValue || statsType.All)
            {
                var string_DailyOnly = ConfigurationManager.AppSettings["FB_DailyStatsOnly"] ?? "";
                Accts_DailyOnly = string_DailyOnly.Split(new char[] { ',' });
            }   // Used when synching all accounts AND/OR all stats types...
            // So if an account is marked as "daily only", you can only load other stats by specifying the accountId and statsType
            // TODO? remove this since we now handle exceptions in the extracter?

            var accounts = GetAccounts();
            foreach (var acct in accounts)
            {
                var acctDateRange = new DateRange(dateRange.FromDate, dateRange.ToDate);
                if (acct.Campaign != null) // check/adjust daterange - if acct assigned to a campaign/advertiser
                {
                    //if FromDate came from the default value, check Advertiser's start date
                    if (!StartDate.HasValue && acct.Campaign.Advertiser.StartDate.HasValue
                        && acctDateRange.FromDate < acct.Campaign.Advertiser.StartDate.Value)
                        acctDateRange.FromDate = acct.Campaign.Advertiser.StartDate.Value;
                    //likewise for ToDate / Advertiser's end date
                    if (!EndDate.HasValue && acct.Campaign.Advertiser.EndDate.HasValue
                        && acctDateRange.ToDate > acct.Campaign.Advertiser.EndDate.Value)
                        acctDateRange.ToDate = acct.Campaign.Advertiser.EndDate.Value;
                }
                Logger.Info("Facebook ETL. Account {0} - {1}. DateRange {2}.", acct.Id, acct.Name, acctDateRange);
                if (acctDateRange.ToDate < acctDateRange.FromDate)
                    continue;

                fbUtility.SetAll();
                if (acct.Network != null)
                {
                    string network = Regex.Replace(acct.Network.Name, @"\s+", "").ToUpper();
                    if (network.StartsWith("FACEBOOK"))
                        fbUtility.SetFacebook();
                    else if (network.StartsWith("INSTAGRAM"))
                        fbUtility.SetInstagram();
                    else if (network.StartsWith("AUDIENCE"))
                        fbUtility.SetAudienceNetwork();
                    else if (network.StartsWith("MESSENGER"))
                        fbUtility.SetMessenger();
                }
                fbUtility.SetCampaignFilter(acct.Filter);

                if (Accts_ConvAsMobAppInst.Contains(acct.ExternalId))
                    fbUtility.Conversion_ActionType = FacebookUtility.Conversion_ActionType_MobileAppInstall;
                else if (Accts_ConvAsPurch.Contains(acct.ExternalId))
                    fbUtility.Conversion_ActionType = FacebookUtility.Conversion_ActionType_Purchase;
                else if (Accts_ConvAsReg.Contains(acct.ExternalId))
                    fbUtility.Conversion_ActionType = FacebookUtility.Conversion_ActionType_Registration;
                else if (Accts_ConvAsVideoPlay.Contains(acct.ExternalId))
                    fbUtility.Conversion_ActionType = FacebookUtility.Conversion_ActionType_VideoPlay;
                else
                    fbUtility.Conversion_ActionType = FacebookUtility.Conversion_ActionType_Default;

                if (Accts_7d_click.Contains(acct.ExternalId))
                    fbUtility.Set_7d_click_attribution();
                else
                    fbUtility.Set_28d_click_attribution(); //default
                if (Accts_7d_view.Contains(acct.ExternalId))
                    fbUtility.Set_7d_view_attribution();
                else
                    fbUtility.Set_1d_view_attribution(); //default

                if (statsType.Daily)
                    DoETL_Daily(acctDateRange, acct, fbUtility);

                if (Accts_DailyOnly.Contains(acct.ExternalId))
                    continue;

                if (statsType.Strategy)
                    DoETL_Strategy(acctDateRange, acct, fbUtility);
                if (statsType.AdSet)
                    DoETL_AdSet(acctDateRange, acct, fbUtility);

                if (statsType.Creative && !statsType.All) // don't include when getting "all" statstypes
                    DoETL_Creative(acctDateRange, acct, fbUtility);
                //if (statsType.Site)
                //    DoETL_Site(acctDateRange, acct, fbUtility);
            }

            return 0;
        }

        private void DoETL_Daily(DateRange dateRange, ExtAccount account, FacebookUtility fbUtility)
        {
            var extracter = new FacebookDailySummaryExtracter(dateRange, account.ExternalId, fbUtility, includeAllActions: false);
            var loader = new FacebookDailySummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        private void DoETL_Strategy(DateRange dateRange, ExtAccount account, FacebookUtility fbUtility)
        {
            var extracter = new FacebookCampaignSummaryExtracter(dateRange, account.ExternalId, fbUtility, includeAllActions: false);
            var loader = new FacebookCampaignSummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        private void DoETL_AdSet(DateRange dateRange, ExtAccount account, FacebookUtility fbUtility)
        {
            var extracter = new FacebookAdSetSummaryExtracter(dateRange, account.ExternalId, fbUtility, includeAllActions: true);
            var loader = new FacebookAdSetSummaryLoader(account.Id, loadActions: true);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }
        private void DoETL_Creative(DateRange dateRange, ExtAccount account, FacebookUtility fbUtility)
        {
            var extracter = new FacebookAdSummaryExtracter(dateRange, account.ExternalId, fbUtility, includeAllActions: false);
            var loader = new FacebookAdSummaryLoader(account.Id);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
        }

        private IEnumerable<ExtAccount> GetAccounts()
        {
            using (var db = new ClientPortalProgContext())
            {
                var accounts = db.ExtAccounts.Include("Network").Include("Campaign.Advertiser").Where(a => a.Platform.Code == Platform.Code_FB);
                if (CampaignId.HasValue || AccountId.HasValue)
                {
                    if (CampaignId.HasValue)
                        accounts = accounts.Where(a => a.CampaignId == CampaignId.Value);
                    if (AccountId.HasValue)
                        accounts = accounts.Where(a => a.Id == AccountId.Value);
                }
                else if (!DisabledOnly)
                    accounts = accounts.Where(a => !a.Disabled);

                if (DisabledOnly)
                    accounts = accounts.Where(a => a.Disabled);

                return accounts.ToList().Where(a => !string.IsNullOrWhiteSpace(a.ExternalId));
            }
        }
    }
}
