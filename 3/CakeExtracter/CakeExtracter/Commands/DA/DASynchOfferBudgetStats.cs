using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CakeExtracter.Bootstrappers;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Reports;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchOfferBudgetStats : ConsoleCommand
    {
        public static int RunStatic(int? offerId)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchOfferBudgetStats()
            {
                OfferId = offerId
            };
            return cmd.Run();
        }

        public int? OfferId { get; set; }
        public bool IncludeRecent { get; set; }
        public int NumParallel { get; set; }
        public int SleepMilliseconds { get; set; }

        private BudgetMonitor budgetMonitor;

        private const int DAYS_RECENT_ACTIVITY = 30;
        //How far back to get stats
        private const int MAX_DAYS_WITH_BUDGET = 93;
        private const int MAX_DAYS_WITHOUTBUDGET = 31;
        private const int DEFAULT_NUM_PARALLEL = 4;

        public override void ResetProperties()
        {
            OfferId = null;
            IncludeRecent = false;
            NumParallel = 0;
            SleepMilliseconds = 0;
        }

        public DASynchOfferBudgetStats()
        {
            IsCommand("daSynchOfferBudgetStats", "synch OfferDailySummaries for all offers with budgets");
            HasOption<int>("o|offerId=", "Offer Id (default = all with budgets)", c => OfferId = c);
            HasOption<bool>("r|recent=", "Include offers with recent activity (default = false)", c => IncludeRecent = c);
            //HasOption<int>("d|days=", "Number of days for recent activity check (default = 31)", c => Days_RecentActivity = c);
            HasOption<int>("p|numParallel=", "Number of offers to do in parallel (default = 4)", c => NumParallel = c);
            HasOption<int>("s|sleepMilliseconds=", "Number of milliseconds to pause between batches of offers (default = 0)", c => SleepMilliseconds = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            budgetMonitor = new BudgetMonitor();

            var offers = GetOffers(OfferId, IncludeRecent); // Fills in current budget stats ("before" values)

            UpdateDailySummariesAndCheckForAlerts(offers);
            // Updates the specified offers' stats using CakeAPI's DailySummaryExport (one call per offer)

            return 0;
        }

        // Get the specified offer, or: the ones with budgets + the ones with recent activity (if specified)
        // Fill in current budget stats
        protected static IEnumerable<Offer> GetOffers(int? offerId, bool includeRecent)
        {
            using (var repo = new DirectAgents.Domain.Concrete.MainRepository(new DAContext()))
            {
                IEnumerable<Offer> offers = new List<Offer>();
                if (offerId.HasValue)
                {
                    var offer = repo.GetOffer(offerId.Value, true, false);
                    if (offer != null)
                        ((List<Offer>)offers).Add(offer);
                }
                else
                {
                    if (includeRecent)
                    {   // Get the ids of offers with recent activity
                        var today = DateTime.Today;
                        var start = today.AddDays(-DAYS_RECENT_ACTIVITY);
                        var dateRange = new DateRange(start, today.AddDays(1));
                        var offerSummaries = CakeMarketingUtility.OfferSummaries(dateRange);
                        var offerIds = offerSummaries.Select(os => os.Offer.OfferId).Where(id => id > -1).ToArray();

                        offers = repo.GetOffersUnion(true, true, true, offerIds); // with budget or with one of the aforementioned ids; exclude 'inactive'
                    }
                    else
                    {
                        offers = repo.GetOffers(true, null, null, true, false, null); // 'active' offers with budget
                    }
                }

                foreach (var offer in offers)
                {
                    repo.FillOfferBudgetStats(offer);
                }
                return offers.ToList();
            }
        }

        private void UpdateDailySummariesAndCheckForAlerts(IEnumerable<Offer> offers)
        {
            if (NumParallel <= 0)
                NumParallel = DEFAULT_NUM_PARALLEL;
            DateTime today = DateTime.Today;
            DateTime minStartWithBudget = today.AddDays(-MAX_DAYS_WITH_BUDGET);
            DateTime minStartWithoutBudget = today.AddDays(-MAX_DAYS_WITHOUTBUDGET);

            bool first = true;
            foreach (var offerBatch in offers.InBatches(NumParallel))
            {
                if (!first && SleepMilliseconds > 0)
                    Thread.Sleep(SleepMilliseconds);
                first = false;

                Parallel.ForEach(offerBatch, offer =>
                {
                    DateTime startDate = offer.DateCreated.Date;
                    DateTime endDate = today;
                    if (offer.HasBudget)
                    {
                        if (offer.BudgetStart.HasValue)
                            startDate = offer.BudgetStart.Value;
                        if (startDate < minStartWithBudget)
                            startDate = minStartWithBudget;
                        //Q: Will this ever be a problem... the budget start is set to yesterday but we don't have daily stats from before that and will miss them?
                    }
                    else
                    {
                        if (startDate < minStartWithoutBudget)
                            startDate = minStartWithoutBudget;
                    }
                    var dateRange = new DateRange(startDate, endDate.AddDays(1));

                    var extracter = new OfferDailySummariesExtracter(dateRange, 0, offer.OfferId, true);
                    var loader = new DAOfferDailySummariesLoader();
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();

                    budgetMonitor.CheckOfferBudgetAlerts(offer); // Updates offer's stats ("after" values) in order to see if an alert is needed
                });
            }
        }

    }

    public class BudgetMonitor
    {
        private GmailEmailer emailer;
        private string reportingEmail;
        private string testEmail, budgetAlertsEmail;

        public BudgetMonitor()
        {
            reportingEmail = ConfigurationManager.AppSettings["GmailReporting_Email"];
            var reportingPassword = ConfigurationManager.AppSettings["GmailReporting_Password"];
            emailer = new GmailEmailer(new System.Net.NetworkCredential(reportingEmail, reportingPassword));

            testEmail = ConfigurationManager.AppSettings["BudgetAlerts_TestEmail"];
            budgetAlertsEmail = ConfigurationManager.AppSettings["BudgetAlerts_Email"];
        }

        public void CheckOfferBudgetAlerts(IEnumerable<Offer> offers)
        {
            using (var repo = new DirectAgents.Domain.Concrete.MainRepository(new DAContext()))
            {
                foreach (var offer in offers)
                    CheckOfferBudgetAlerts(offer, repo);
            }
        }

        public void CheckOfferBudgetAlerts(Offer offer, DirectAgents.Domain.Concrete.MainRepository mainRepo = null)
        {
            //TODO? use 1.0m ? (two places)

            if (!offer.BudgetUsedPercent.HasValue || offer.BudgetUsedPercent.Value >= 1)
                return; // null or already over 100%
            decimal oldPercentUsed = offer.BudgetUsedPercent.Value;
            decimal oldBudgetUsed = offer.BudgetUsed ?? 0;

            if (mainRepo == null)
            {
                using (var repo = new DirectAgents.Domain.Concrete.MainRepository(new DAContext()))
                {
                    repo.FillOfferBudgetStats(offer);
                }
            }
            else
            {
                mainRepo.FillOfferBudgetStats(offer);
            }
            if (!offer.BudgetUsedPercent.HasValue)
                return; // this should never happen

            decimal newPercentUsed = offer.BudgetUsedPercent.Value;
            decimal newBudgetUsed = offer.BudgetUsed ?? 0;
            if (newPercentUsed >= 1)
            {
                // went over 100%
                Logger.Info("Offer {0} reached 100% of budget. Prev: {1:N2} ({2:P1}) Now: {3:N2} ({4:P1})",
                            offer.OfferId, oldBudgetUsed, oldPercentUsed, newBudgetUsed, newPercentUsed);
                SendAlertEmail(offer, true);
            }
            else if (oldPercentUsed < .85m && newPercentUsed >= .85m)
            {
                // went over 85%
                Logger.Info("Offer {0} reached 85% of budget. Prev: {1:N2} ({2:P1}) Now: {3:N2} ({4:P1})",
                            offer.OfferId, oldBudgetUsed, oldPercentUsed, newBudgetUsed, newPercentUsed);
                SendAlertEmail(offer, false);
            }
        }

        private void SendAlertEmail(Offer offer, bool over100percent)
        {
            string subject = String.Format("Budget Alert: {0}% reached for: {1}", (over100percent ? "100" : "85"), offer.OfferName);

            var adv = offer.Advertiser;
            StringBuilder bodyText = new StringBuilder();
            if (over100percent)
                bodyText.Append("*** Reached 100% of budget. ***<br/>Please pause all partners.<br/><br/>");
            else
                bodyText.Append("*** Reached 85% of budget. ***<br/>Please do not set any new partners live at this time.<br/><br/>");

            bodyText.Append("<table>");
            bodyText.AppendFormat("<tr><td>Offer:</td><td>{0}</td></tr>", offer.OfferName);
            bodyText.AppendFormat("<tr><td>OfferId:</td><td>{0}</td></tr>", offer.OfferId);
            bodyText.AppendFormat("<tr><td>Budget:</td><td>${0:N2}</td></tr>", offer.Budget);
            bodyText.AppendFormat("<tr><td>Budget Used:</td><td>${0:N2}</td></tr>", offer.BudgetUsed);
            bodyText.AppendFormat("<tr><td>Percent Used:</td><td>{0:P1}</td></tr>", offer.BudgetUsedPercent);
            bodyText.AppendFormat("<tr><td>Stats Range:</td><td>{0} - {1}</td></tr>",
                                  offer.EarliestStatDate.HasValue ? offer.EarliestStatDate.Value.ToShortDateString() : "",
                                  offer.LatestStatDate.HasValue ? offer.LatestStatDate.Value.ToShortDateString() : "");
            bodyText.AppendFormat("<tr><td>BudgetStart:</td><td>{0}</td></tr>", offer.BudgetStart.HasValue ? offer.BudgetStart.Value.ToShortDateString() : "");
            bodyText.AppendFormat("<tr><td>BudgetEnd:</td><td>{0}</td></tr>", offer.BudgetEnd.HasValue ? offer.BudgetEnd.Value.ToShortDateString() : "");

            string adManagerText = adv.AdManager == null ? "" : String.Format("{0} ({1})", adv.AdManager.FullName, adv.AdManager.EmailAddress);
            bodyText.AppendFormat("<tr><td>Ad Manager:</td><td>{0}</td></tr>", adManagerText);
            string acctManagerText = adv.AccountManager == null ? "" : String.Format("{0} ({1})", adv.AccountManager.FullName, adv.AccountManager.EmailAddress);
            bodyText.AppendFormat("<tr><td>Acct Manager:</td><td>{0}</td></tr>", acctManagerText);
            bodyText.Append("</table>");

            string toAddress;
            if (String.IsNullOrWhiteSpace(testEmail))
            {
                toAddress = budgetAlertsEmail + "," + (adv.AdManager != null ? adv.AdManager.EmailAddress : "")
                    + "," + (adv.AccountManager != null ? adv.AccountManager.EmailAddress : "");
            }
            else
            {
                toAddress = testEmail;
            }
            emailer.SendEmail(reportingEmail,
                              new string[] { toAddress },
                              null,
                              subject,
                              bodyText.ToString(),
                              true);
        }
    }
}
