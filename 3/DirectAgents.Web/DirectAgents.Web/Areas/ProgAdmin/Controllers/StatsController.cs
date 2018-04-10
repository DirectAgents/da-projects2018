using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Web.Areas.ProgAdmin.Models;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class StatsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public StatsController(ICPProgRepository cpProgRepository, ITDRepository tdRepository)
        {
            this.cpProgRepo = cpProgRepository;
            this.tdRepo = tdRepository;
        }

        public ActionResult MTD(int campId, DateTime? date) // Month/MTD view
        {
            if (!date.HasValue)
                date = DateTime.Today;
            var startOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);

            var campStat = cpProgRepo.GetCampStats(startOfMonth, campId);
            if (campStat == null)
                return HttpNotFound();

            return View(campStat);
        }

        // Stats by "external account"
        public ActionResult ExtAccount(string platform, int? campId, int? acctId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = new List<TDRawStat>();

            var extAccounts = cpProgRepo.ExtAccounts(platformCode: platform, campId: campId)
                                .OrderBy(a => a.Platform.Code).ThenBy(a => a.Name);
            if (acctId.HasValue)
                extAccounts = extAccounts.Where(a => a.Id == acctId.Value).OrderBy(a => a.Name); // will be only one

            foreach (var extAccount in extAccounts)
            {   //Note: Multiple Active Record Sets used here
                var stat = cpProgRepo.GetTDStatWithAccount(startOfMonth, endOfMonth, extAccount: extAccount);
                if (!stat.AllZeros())
                    stats.Add(stat);
            }
            var model = new TDStatsVM
            {
                //Name =
                Month = startOfMonth,
                PlatformCode = platform,
                CampaignId = campId,
                AccountId = acctId,
                Stats = stats
            };
            return View(model);
        }

        // Stats by "strategy"
        public ActionResult Strategy(int? acctId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = cpProgRepo.GetStrategyStats(startOfMonth, endOfMonth, acctId: acctId)
                .OrderBy(s => s.Strategy.Name).ThenBy(s => s.Strategy.Id);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                Stats = stats
            };
            return View(model);
        }

        // Stats by "TDad"
        public ActionResult Creative(int? acctId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = cpProgRepo.GetTDadStats(startOfMonth, endOfMonth, acctId: acctId)
                .OrderBy(s => s.TDad.Name).ThenBy(s => s.TDad.Id);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                Stats = stats
            };
            return View(model);
        }

        // Stats by AdSet
        public ActionResult AdSet(int? acctId, int? stratId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = cpProgRepo.GetAdSetStats(startOfMonth, endOfMonth, acctId: acctId, stratId: stratId)
                .OrderBy(s => s.AdSet.Name).ThenBy(s => s.AdSet.Id);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                StratId = stratId,
                Stats = stats
            };
            return View(model);
        }

        // Stats by site
        public ActionResult Site(int? acctId, DateTime? month, int? minImp)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = cpProgRepo.GetSiteStats(startOfMonth, endOfMonth, acctId: acctId, minImpressions: minImp)
                .OrderByDescending(s => s.Impressions).ThenBy(s => s.Site.Name);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                Stats = stats
            };
            return View(model);
        }

        public ActionResult Conv(int? acctId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var convs = cpProgRepo.Convs(startOfMonth, endOfMonth, acctId: acctId)
                .OrderBy(s => s.Time);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                Convs = convs
            };
            return View(model);
        }

        // Stats by ActionType
        public ActionResult Action(int? acctId, int? stratId, int? adsetId, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = cpProgRepo.GetAdSetActionStats(startOfMonth, endOfMonth, acctId: acctId, stratId: stratId, adsetId: adsetId)
                .OrderBy(s => s.ActionType.Code);

            var model = new TDStatsVM
            {
                Month = startOfMonth,
                AccountId = acctId,
                StratId = stratId,
                AdSetId = adsetId,
                Stats = stats
            };
            return View(model);
        }

        //TODO: remove all this... (stuff that uses tdRepo)

        // AdRoll Stats by Ad
        public ActionResult AdRoll(string advEid, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var stats = new List<TDRawStat>();

            var advertisable = tdRepo.Advertisable(advEid);
            if (advertisable == null)
                return Content("not found");

            var ads = tdRepo.AdRoll_Ads(advEid: advEid);
            foreach (var ad in ads)
            {   //Note: Multiple Active Record Sets used here
                var stat = tdRepo.GetAdRollStat(ad, startOfMonth, endOfMonth);
                if (!stat.AllZeros())
                    stats.Add(stat);
            }
            var model = new TDStatsVM
            {
                Name = "ExtAccount: " + advertisable.Name + "(AdRoll)",
                ExternalId = advEid,
                Month = startOfMonth,
                Stats = stats.OrderBy(a => a.Name).ToList()
            };
            return View(model);
        }

        // DBM Stats by Creative
        public ActionResult DBM(int ioID, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var insertionOrder = tdRepo.InsertionOrder(ioID);
            if (insertionOrder == null)
                return Content("not found");

            var stats = tdRepo.GetDBMStatsByCreative(ioID, startOfMonth, endOfMonth);
            var model = new TDStatsVM
            {
                Name = "ExtAccount: " + insertionOrder.Name + "(DBM)",
                ExternalId = ioID.ToString(),
                Month = startOfMonth,
                Stats = stats.OrderBy(a => a.Name).ToList()
            };
            return View("Generic", model);
        }

        // Demo - DBM stats by creative
        public ActionResult Spreadsheet(int ioID, DateTime? month)
        {
            return View(ioID);
        }
        public JsonResult SpreadsheetData(int ioID, DateTime? month)
        {
            var startOfMonth = SetChooseMonthViewData_NonCookie("RT", month);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var insertionOrder = tdRepo.InsertionOrder(ioID);
            if (insertionOrder == null)
                return null;

            var stats = tdRepo.GetDBMStatsByCreative(ioID, startOfMonth, endOfMonth);
            var json = Json(stats, JsonRequestBehavior.AllowGet);
            //var json = Json(stats);
            return json;
        }

        public JsonResult Test()
        {
            var advId = 18;
            var startDate = new DateTime(2016, 3, 1);
            var endDate = new DateTime(2016, 3, 21);
            var stats = cpProgRepo.DayOfWeekBasicStats(advId, startDate, endDate);
            //var stats = cpProgRepo.DailySummaryBasicStats(advId, startDate, endDate);
            var json = Json(stats, JsonRequestBehavior.AllowGet);
            //var json = Json(stats);
            return json;
        }
    }
}