using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.DTO;
using DirectAgents.Web.Areas.AB.Models;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class DashboardController : DirectAgents.Web.Controllers.ControllerBase
    {
        public DashboardController(IMainRepository daRepository, IRevTrackRepository rtRepository, IABRepository abRepository, ISuperRepository superRepository)
        {
            this.daRepo = daRepository;
            this.rtRepo = rtRepository;
            this.abRepo = abRepository;
            this.superRepo = superRepository;
        }

        public ActionResult ProgTest(int id)
        {
            var monthStart = new DateTime(2016, 12, 1); //testing
            var pStats = rtRepo.GetProgClientStats(monthStart, id);

            return null;
        }

        public ActionResult ChooseMonth(DateTime month)
        {
            SetCurrentMonth("AB", month);
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Index(int x = 10)
        {
            DateTime currMonth = SetChooseMonthViewData("AB");

            var model = new DashboardVM
            {
                ABStats = GetStatsByClient(currMonth, x)
            };
            return View(model);
        }
        private IEnumerable<ABStat> GetStatsByClient(DateTime monthStart, int? maxClients = null)
        {
            var clientStats = superRepo.StatsByClient(monthStart, maxClients: maxClients);
            return clientStats;
        }

        //Breakdown by department/source...
        public ActionResult BySource(int clientId)
        {
            var client = abRepo.Client(clientId);
            if (client == null)
                return HttpNotFound();

            DateTime currMonth = SetChooseMonthViewData("AB");

            var model = new DashboardVM
            {
                ABClient = client,
                ABStats = superRepo.StatsForClient(clientId, currMonth)
            };
            return View(model);
        }

        public ActionResult LineItems(int clientId)
        {
            var client = abRepo.Client(clientId);
            if (client == null)
                return HttpNotFound();

            DateTime currMonth = SetChooseMonthViewData("AB");

            var lineItems = superRepo.StatsByLineItem(clientId, currMonth);

            var model = new DashboardVM
            {
                ABClient = client,
                LineItems = lineItems
            };
            return View(model);
        }

        // New version of Single Client Detail view
        public ActionResult Proto(int id)
        {
            var client = abRepo.Client(id);
            if (client == null)
                return HttpNotFound();

            //For now, we're going through all Periods in the db...

            var periods = abRepo.ProtoPeriods();
            var periodGroups = new List<PeriodGroup>();

            // Get "active" accounts
            var spendBits = abRepo.ProtoSpendBits(clientId: id);
            var accounts = spendBits.Select(x => x.ProtoCampaign.ClientAccount).Distinct().OrderBy(a => a.Id).ToList();
            // ? better to say something like client.Accounts.where(x => x.camps.spendbits.any()) ?

            foreach (var period in periods.OrderBy(x => x.Date))
            {
                //var spendBits = abRepo.SpendBits(clientId: id, periodId: period.Id);
                //var accounts = spendBits.Select(x => x.ProtoCampaign.ClientAccount).Distinct().OrderBy(x => x.Id).ToList();
                var accountGroups = new List<AccountGroup>();
                foreach (var account in accounts)
                {
                    var sBits = spendBits.Where(x => x.ProtoPeriodId == period.Id && x.ProtoCampaign.ClientAccountId == account.Id);
                    var accountGroup = new AccountGroup
                    {
                        Name = account.Name,
                        LineItems = sBits.OrderBy(x => x.Id).ToList().Select(sb => new ABLineItem(sb))
                    };
                    accountGroups.Add(accountGroup);
                }
                var periodGroup = new PeriodGroup
                {
                    Month = period.Date,
                    AccountGroups = accountGroups
                };
                periodGroups.Add(periodGroup);
            }
            var model = new ProtoVM
            {
                ABClient = client,
                PeriodGroups = periodGroups
            };
            return View(model);
        }

        // Client Summary - multiple months
        public ActionResult Client(int id)
        {
            var client = abRepo.Client(id);
            if (client == null)
                return HttpNotFound();

            var monthGroups = new List<MonthGroup>();
            int numMonths = 3; //TODO: determine this elsewhere

            var firstDate = DateTime.Today.FirstDayOfMonth(addMonths: -numMonths);
            var lastDate = firstDate.AddMonths(numMonths).AddDays(-1);
            var jobs = abRepo.ActiveJobs(id, firstDate, lastDate).OrderBy(j => j.Name);

            var month = firstDate;
            for (int i = 0; i < numMonths; i++) // get lineitems for last three months
            {
                var monthStats = superRepo.StatsByLineItem(id, month, separateFees: true);
                var jobGroups = new List<JobGroup>();
                var extraItems = abRepo.ExtraItems(month, month.AddMonths(1).AddDays(-1));
                foreach (var job in jobs)
                {
                    var jobExtraItems = extraItems.Where(x => x.JobId == job.Id);
                    var jobGroup = new JobGroup
                    {
                        Job = job,
                        LineItems = jobExtraItems.OrderBy(x => x.Id).ToList().Select(x => new ABLineItem(x))
                    };
                    jobGroups.Add(jobGroup);
                }
                var monthGroup = new MonthGroup
                {
                    Month = month,
                    LineItems = monthStats,
                    JobGroups = jobGroups
                };
                monthGroups.Add(monthGroup);
                month = month.AddMonths(1);
            }

            var model = new DetailVM
            {
                ABClient = client,
                MonthGroups = monthGroups
            };

            //TODO: get summary info for the client
            // ??? by lineitem(<-try this) or by vendor ?
            // (for cake, need offaff dailysums)
            // superRepo.StatsForClient?
            // payments for the current month
            // somehow get a starting balance; go back to beginning of last month... for now, start at zero
            // ...later, when we have a StartingBalance entity: if there's one for the beginning of this month, use it. otherwise try going back
            // ...to the beginning of last month and see if there's one there.  (?check if any mid-month?)  if none found, use zero.
            // [Check AccountingBackup sheets for what's in included on the page.]
            //
            // # units?

            return View(model);
        }
    }
}