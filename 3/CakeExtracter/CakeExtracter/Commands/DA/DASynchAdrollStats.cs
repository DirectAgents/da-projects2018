using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AdRoll;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchAdrollStats : ConsoleCommand
    {
        // if Eid not specified, will ask AdRoll for all Advertisables that have stats
        public static int RunStatic(int? accountId = null, DateTime? startDate = null, DateTime? endDate = null, string oneStatPer = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchAdrollStats
            {
                AccountId = accountId,
                CheckActiveAdvertisables = !accountId.HasValue,
                StartDate = startDate,
                EndDate = endDate,
                OneStatPer = oneStatPer
            };
            return cmd.Run();
        }

        //TODO: Change "OneStatPer" to "StatsType" (note: RunStatic is called from DAWeb)

        public int? AccountId { get; set; }
        public int? AdvertisableId { get; set; }
        public string AdvertisableEids { get; set; } // if NullOrWhitespace, gets filled in during Run()
        public string CampaignEids { get; set; }
        public string ExternalCampaignEids { get; set; } // for newer facebook campaigns; used in DoETL_CampaignLevel
        public bool CheckActiveAdvertisables { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysAgoToStart { get; set; }
        public string OneStatPer { get; set; } // (per day)
        public bool UpdateAds { get; set; }
        public bool UpdateAdvertisables { get; set; }
        public bool DisabledOnly { get; set; }

        public override void ResetProperties()
        {
            AccountId = null;
            AdvertisableId = null;
            AdvertisableEids = null;
            CampaignEids = null;
            ExternalCampaignEids = null;
            CheckActiveAdvertisables = false;
            StartDate = null;
            EndDate = null;
            DaysAgoToStart = null;
            OneStatPer = null;
            UpdateAds = false;
            UpdateAdvertisables = false;
            DisabledOnly = false;
        }

        public DASynchAdrollStats()
        {
            IsCommand("daSynchAdrollStats", "synch AdRoll Stats");
            HasOption<int>("a|accountId=", "Account Id (default = all)", c => AccountId = c); // takes precedence over advertisableEids
            //HasOption("x|advertisableEids=", "Advertisable Eids (comma-separated) (default = all)", c => AdvertisableEids = c);
            HasOption<int>("i|advertisableId=", "Advertisable Id (default = all)", c => AdvertisableId = c);
            HasOption("m|campaignEids=", "Campaign Eids (comma-separated) (default = all)", c => CampaignEids = c);
            HasOption("n|externalCampaignEids=", "ExternalCampaign Eids (comma-separated) (default = all)", c => ExternalCampaignEids = c);
            HasOption("c|checkActive=", "Check AdRoll for Advertisables with stats (if none specified)", c => CheckActiveAdvertisables = bool.Parse(c));
            HasOption("s|startDate=", "Start Date (default is 'daysAgo')", c => StartDate = DateTime.Parse(c));
            HasOption("e|endDate=", "End Date (default is yesterday)", c => EndDate = DateTime.Parse(c));
            HasOption<int>("d|daysAgo=", "Days Ago to start, if startDate not specified (default = 31)", c => DaysAgoToStart = c);
            HasOption("o|oneStatPer=", "One Stat per [what] per day (advertisable/campaign/ad, default = all)", c => OneStatPer = c);
            HasOption("u|updateAds=", "After synching ad stats, update Ads? (default = false)", c => UpdateAds = bool.Parse(c));
            HasOption("z|updateAdvertisables=", "Before synching stats, update Advertisables? (default = false)", c => UpdateAdvertisables = bool.Parse(c));
            HasOption<bool>("x|disabledOnly=", "Include only disabled accounts (default = false)", c => DisabledOnly = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            if (!DaysAgoToStart.HasValue)
                DaysAgoToStart = 31; // used if StartDate==null
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dateRange = new DateRange(StartDate ?? today.AddDays(-DaysAgoToStart.Value), EndDate ?? yesterday);
            Logger.Info("AdRoll ETL. DateRange {0}.", dateRange);

            var arUtility = new AdRollUtility(m => Logger.Info(m), m => Logger.Warn(m));
            if (UpdateAdvertisables)
            {
                Logger.Info("Updating Advertisables...");
                DASynchAdrollAccounts.UpdateAdvertisables(arUtility);
            }
            if (AccountId.HasValue) // For now, try getting advertisable Eid from accountId, if specified
            {
                using (var db = new ClientPortalProgContext())
                {
                    var extAcct = db.ExtAccounts.Find(AccountId.Value);
                    if (extAcct != null)
                        AdvertisableEids = extAcct.ExternalId; //Note: overwrites any AdvertisableEids set by the caller
                }
            }

            IEnumerable<Advertisable> advertisables;
            if (CheckActiveAdvertisables && string.IsNullOrWhiteSpace(AdvertisableEids) && !AdvertisableId.HasValue)
                advertisables = GetAdvertisablesThatHaveStats(dateRange, arUtility);
            else
                advertisables = GetAdvertisables();

            // If specified certain account(s), don't worry about the Disabled property... unless DisabledOnly is set
            var advSpecified = (!String.IsNullOrWhiteSpace(AdvertisableEids) || AdvertisableId.HasValue);
            if (!advSpecified || DisabledOnly)
            {
                // Filter 'Disabled' accounts. (also removes advertisables whose Eid has been nulled out)
                using (var db = new ClientPortalProgContext())
                {
                    var extAccounts = db.ExtAccounts.Where(a => a.Platform.Code == Platform.Code_AdRoll && a.Disabled == DisabledOnly);
                    var externalIds = extAccounts.Select(a => a.ExternalId).ToList()
                        .Where(i => !String.IsNullOrWhiteSpace(i));
                    advertisables = advertisables.Where(a => externalIds.Contains(a.Eid));
                }
            }

            if (string.IsNullOrWhiteSpace(AdvertisableEids))
                AdvertisableEids = String.Join(",", advertisables.Select(a => a.Eid)); // this needed?

            string _oneStatPer = (OneStatPer == null) ? "" : OneStatPer.ToLower();
            if (_oneStatPer.StartsWith("adv") || _oneStatPer == "")
                DoETL_AdvertisableLevel(dateRange, advertisables);
            if (_oneStatPer.StartsWith("camp") || _oneStatPer == "")
                DoETL_CampaignLevel(dateRange, advertisables);
            if ((_oneStatPer.StartsWith("ad") && !_oneStatPer.StartsWith("adv")) || _oneStatPer == "")
                DoETL_AdLevel(dateRange, advertisables);

            return 0;
        }

        private void DoETL_AdvertisableLevel(DateRange dateRange, IEnumerable<Advertisable> advertisables, AdRollUtility arUtility = null)
        {
            //TODO: If there are fewer dates than advertisables, can we loop through the dates and make one API call for each date?

            foreach (var adv in advertisables)
            {
                var extracter = new AdrollDailySummariesExtracter(dateRange, adv.Eid, arUtility);
                var loader = new AdrollDailySummaryLoader(adv.Eid);
                if (loader.FoundAccount())
                {
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();

                    // Get attribution vals (client's revenue)
                    var attrExtracter = new AdrollAttributionSummariesExtracter(dateRange, adv.Eid, arUtility);
                    var attrLoader = new AdrollAttributionSummaryLoader(loader.AccountId);
                    extracterThread = attrExtracter.Start();
                    loaderThread = attrLoader.Start(attrExtracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
                else
                    Logger.Warn("AdRoll Account did not exist for Advertisable with Eid {0}. Cannot do ETL.", adv.Eid);
            }
        }

        // Extracts campaign stats, converts campaign to strategy during load
        private void DoETL_CampaignLevel(DateRange dateRange, IEnumerable<Advertisable> advertisables, AdRollUtility arUtility = null)
        {
            //TODO: same as AdLevel todo
            foreach (var adv in advertisables)
            {
                var extracter = new AdrollCampaignDailySummariesExtracter(dateRange, adv.Eid, arUtility, campaignEid: this.CampaignEids, externalCampaignEid: this.ExternalCampaignEids);
                var loader = new AdrollCampaignSummaryLoader(adv.Eid);
                if (loader.FoundAccount())
                {
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();
                }
                else
                    Logger.Warn("AdRoll Account did not exist for Advertisable with Eid {0}. Cannot do ETL.", adv.Eid);
            }
        }

        private void DoETL_AdLevel(DateRange dateRange, IEnumerable<Advertisable> advertisables, AdRollUtility arUtility = null)
        {
            //TODO: Loop thru dateRange. For each date, make one API call.
            //      (Need to update Loader- pass in advertisables(?)... needed for creating new Ads in the DB... Items have Advertisable *name*, not Eid.)

            foreach (var adv in advertisables)
            {
                ExtAccount extAccount = null;
                using (var db = new ClientPortalProgContext())
                {
                    extAccount = db.ExtAccounts
                        .Where(a => a.ExternalId == adv.Eid && a.Platform.Code == Platform.Code_AdRoll && !a.Disabled)
                        .FirstOrDefault();
                }
                if (extAccount != null)
                {
                    var extracter = new AdrollAdDailySummariesExtracter(dateRange, adv.Eid, arUtility);
                    var loader = new AdrollAdSummaryLoader(extAccount.Id);
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();

                    if (UpdateAds)
                    {
                        var adExtracter = new AdRollAdExtracter(loader.AdEids, arUtility: arUtility);
                        var adLoader = new AdRollAdLoader(extAccount.Id);
                        var adExtracterThread = adExtracter.Start();
                        var adLoaderThread = adLoader.Start(adExtracter);
                        adExtracterThread.Join();
                        adLoaderThread.Join();
                    }
                }
                else
                    Logger.Warn("AdRoll Account did not exist for Advertisable with Eid {0}. Cannot do ETL.", adv.Eid);
            }
        }
        private void DoETL_AdLevelOLD(DateRange dateRange, IEnumerable<Advertisable> advertisables, AdRollUtility arUtility = null)
        {
            foreach (var adv in advertisables)
            {
                var extracter = new AdrollAdDailySummariesExtracter(dateRange, adv.Eid, arUtility);
                var loader = new AdrollAdDailySummaryLoader(adv.Id);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
        }

        public IEnumerable<Advertisable> GetAdvertisables()
        {
            string[] advEidsArray = new string[] { };
            if (!string.IsNullOrWhiteSpace(this.AdvertisableEids))
                advEidsArray = this.AdvertisableEids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            using (var db = new ClientPortalProgContext())
            {
                var advs = db.Advertisables.AsQueryable();
                if (advEidsArray.Any() && this.AdvertisableId.HasValue)
                {   // Handles if advs are specified by both Eid and Id
                    advs = advs.Where(a => advEidsArray.Contains(a.Eid) || a.Id == this.AdvertisableId.Value);
                }
                else if (advEidsArray.Any())
                {
                    advs = advs.Where(a => advEidsArray.Contains(a.Eid));
                }
                else if (this.AdvertisableId.HasValue)
                {
                    advs = advs.Where(a => a.Id == this.AdvertisableId.Value);
                }
                return advs.ToList();
            }
        }

        public IEnumerable<Advertisable> GetAdvertisablesThatHaveStats(DateRange dateRange, AdRollUtility arUtility)
        {
            //TODO? call arUtility.GetAdvertisables() and take the intersection of those Eids and those in the db

            IEnumerable<Advertisable> advertisables;
            using (var db = new ClientPortalProgContext())
            {
                advertisables = db.Advertisables.ToList();
                advertisables = advertisables.Where(a => !string.IsNullOrWhiteSpace(a.Eid));
            }
            var dbAdvEids = advertisables.Select(a => a.Eid).ToArray();
            var advSums = arUtility.AdvertisableSummaries(dateRange.FromDate, dateRange.ToDate, dbAdvEids);
            var advEidsThatHaveStats = advSums.Where(s => !s.AllZeros(includeProspects: true)).Select(s => s.eid).ToArray();
            advertisables = advertisables.Where(a => advEidsThatHaveStats.Contains(a.Eid));
            return advertisables;
        }
    }
}
