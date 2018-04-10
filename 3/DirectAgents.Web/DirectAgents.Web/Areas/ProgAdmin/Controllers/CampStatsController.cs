using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.DTO;
using DirectAgents.Web.Areas.ProgAdmin.Models;
using KendoGridBinderEx;
using KendoGridBinderEx.ModelBinder.Mvc;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class CampStatsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public CampStatsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        // HTML version
        public ActionResult Pacing(int? campId, bool showPerfStats = false)
        {
            DateTime currMonth = SetChooseMonthViewData("RT");
            var campStats = GetCampStats(currMonth, campId);
            var model = new CampaignPacingVM
            {
                CampStats = campStats,
                ShowPerfStats = showPerfStats
            };
            return View(model);
        }
        // TODO: Work on this... For one campaign, how did it do each month?
        //public ActionResult PacingByMonth(int campId)
        //{
        //    //var r = cpProgRepo.StatsRange_All(
        //    return null;
        //}

        public ActionResult PacingGrid()
        {
            SetChooseMonthViewData("RT");
            return View();
        }
        //[HttpPost]
        public JsonResult PacingData(KendoGridMvcRequest request) //, DateTime? month)
        {
            // The "agg" aggregates will get computed outside of KendoGridEx
            if (request.AggregateObjects != null)
                request.AggregateObjects = request.AggregateObjects.Where(ao => ao.Aggregate != "agg");

            var startOfMonth = GetCurrentMonth("RT");
            var campStats = GetCampStats(startOfMonth);
            var dtos = campStats.Select(s => new CampaignPacingDTO(s));
            var kgrid = new KendoGridEx<CampaignPacingDTO>(request, dtos);

            return CreateJsonResult(kgrid, Aggregates(kgrid), allowGet: true);
        }
        //[HttpPost]
        public JsonResult PacingDetail(KendoGridMvcRequest request, int campId) //, DateTime? month)
        {
            DateTime currMonth = GetCurrentMonth("RT");
            var stat = cpProgRepo.GetCampStats(currMonth, campId);
            var dtos = stat.LineItems.Select(li => new CampaignPacingDTO(li));
            var kgrid = new KendoGridEx<CampaignPacingDTO>(request, dtos);

            return CreateJsonResult(kgrid, Aggregates(kgrid), allowGet: true);
        }

        public ActionResult PerformanceGrid()
        {
            SetChooseMonthViewData("RT");
            return View();
        }
        //[HttpPost]
        public JsonResult PerformanceData(KendoGridMvcRequest request) //, DateTime? month)
        {
            // The "agg" aggregates will get computed outside of KendoGridEx
            if (request.AggregateObjects != null)
                request.AggregateObjects = request.AggregateObjects.Where(ao => ao.Aggregate != "agg");

            var startOfMonth = GetCurrentMonth("RT");
            var campStats = GetCampStats(startOfMonth);
            var dtos = campStats.Select(s => new PerformanceDTO(s));
            var kgrid = new KendoGridEx<PerformanceDTO>(request, dtos);

            return CreateJsonResult(kgrid, Aggregates(kgrid), allowGet: true);
        }

        private IEnumerable<TDCampStats> GetCampStats(DateTime startOfMonth, int? campId = null)
        {
            DateTime currMonth = GetCurrentMonth("RT");

            var campaigns = cpProgRepo.Campaigns();
            if (campId.HasValue)
                campaigns = campaigns.Where(c => c.Id == campId.Value); // the specified campaign
            else
                campaigns = campaigns.Where(c => !c.Name.Contains("Demo")); // all but the demo campaigns

            var campStatsList = new List<TDCampStats>();
            foreach (var camp in campaigns.OrderBy(c => c.Advertiser.Name).ThenBy(c => c.Name))
            {
                var stat = cpProgRepo.GetCampStats(currMonth, camp.Id);
                if (!stat.AllZeros())
                    campStatsList.Add(stat);
                // TODO: include campStats for campaigns with BudgetInfos (or PlatformBudgetInfos?), even if no stats ?
            }
            return campStatsList;
        }

        // Budget & Pacing
        public ActionResult Spreadsheet(int campId)
        {
            return View(campId);
        }
        public JsonResult SpreadsheetData(int campId)
        {
            DateTime currMonth = GetCurrentMonth("RT");
            var campStats = cpProgRepo.GetCampStats(currMonth, campId);

            var dtos = new List<CampaignPacingDTO> { new CampaignPacingDTO(campStats) };
            foreach (var li in campStats.LineItems)
            {
                dtos.Add(new CampaignPacingDTO(li));
            }
            var json = Json(dtos, JsonRequestBehavior.AllowGet);
            //var json = Json(dtos);
            return json;
        }

        // Daily Stats Spreadsheet
        public ActionResult Daily(int campId, DateTime? start, DateTime? end)
        {
            var campaign = cpProgRepo.Campaign(campId);
            if (campaign == null)
                return HttpNotFound();
            var model = new ReportingVM
            {
                Campaign = campaign,
                Start = start,
                End = end,
                ColumnConfigs = ColumnConfig.BasicColumns()
            };
            return View(model);
        }
        //[HttpPost]
        public JsonResult DailyData(int campId, DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
                start = GetCurrentMonth("RT");
            var dailyDTOs = GetDailyStatsDTO(campId, start, end);

            var json = Json(dailyDTOs, JsonRequestBehavior.AllowGet);
            //var json = Json(dailyDTOs);
            return json;
        }

        // Daily Stats line chart
        public ActionResult DailyChart(int campId)
        {
            var campaign = cpProgRepo.Campaign(campId);
            if (campaign == null)
                return HttpNotFound();
            return View(campaign);
        }
        //[HttpPost]
        public JsonResult DailyChartData(KendoGridMvcRequest request, int campId)
        {
            DateTime start = GetCurrentMonth("RT").AddMonths(-1);
            var dailyDTOs = GetDailyStatsDTO(campId, start, null);
            var kgrid = new KendoGridEx<DailyDTO>(request, dailyDTOs);
            return CreateJsonResult(kgrid, null, allowGet: true);
        }

        private IEnumerable<DailyDTO> GetDailyStatsDTO(int campId, DateTime? startDate, DateTime? endDate)
        {
            var dailyLIs = cpProgRepo.GetDailyStatsLI(campId, startDate, endDate);
            var dailyDTOs = dailyLIs.Select(li => new DailyDTO
            {
                Date = li.Date.Value,
                Impressions = li.Impressions,
                Clicks = li.Clicks,
                TotalConv = li.PostClickConv + li.PostViewConv,
                PostClickConv = li.PostClickConv,
                PostViewConv = li.PostViewConv,
                Spend = li.ClientCost,
                CTR = li.CTR,
                CPA = li.CPA
            });
            return dailyDTOs;
        }

        // T could be CampaignPacingDTO, PerformanceDTO...
        public static object Aggregates<T>(KendoGridEx<T> kgrid)
        {
            if (kgrid.Total == 0 || kgrid.Aggregates == null) return null;

            decimal budget = ((dynamic)kgrid.Aggregates)["Budget"]["sum"];
            decimal daCost = ((dynamic)kgrid.Aggregates)["DACost"]["sum"];
            decimal clientCost = ((dynamic)kgrid.Aggregates)["ClientCost"]["sum"];
            decimal totalRev = ((dynamic)kgrid.Aggregates)["TotalRev"]["sum"];
            decimal margin = ((dynamic)kgrid.Aggregates)["Margin"]["sum"];

            decimal? marginPct = null;
            if (totalRev != 0)
                marginPct = 1 - daCost / totalRev;

            decimal? pctOfGoal = null;
            if (budget != 0)
                pctOfGoal = clientCost / budget;

            if (typeof(T) == typeof(CampaignPacingDTO))
            {
                var aggs = new
                {
                    Budget = new { sum = budget },
                    DACost = new { sum = daCost },
                    ClientCost = new { sum = clientCost },
                    TotalRev = new { sum = totalRev },
                    Margin = new { sum = margin },
                    MarginPct = new { agg = marginPct },
                    PctOfGoal = new { agg = pctOfGoal }
                };
                return aggs;
            }
            else if (typeof(T) == typeof(PerformanceDTO))
            {
                int impressions = ((dynamic)kgrid.Aggregates)["Impressions"]["sum"];
                int clicks = ((dynamic)kgrid.Aggregates)["Clicks"]["sum"];
                int totalConv = ((dynamic)kgrid.Aggregates)["TotalConv"]["sum"];
                int postClickConv = ((dynamic)kgrid.Aggregates)["PostClickConv"]["sum"];
                int postViewConv = ((dynamic)kgrid.Aggregates)["PostViewConv"]["sum"];

                double? ctr = null;
                if (impressions != 0)
                    ctr = (double)clicks / impressions;

                decimal? cpa = null;
                if (totalConv != 0)
                    cpa = clientCost / totalConv;

                var aggs = new
                {
                    Budget = new { sum = budget },
                    DACost = new { sum = daCost },
                    ClientCost = new { sum = clientCost },
                    TotalRev = new { sum = totalRev },
                    Margin = new { sum = margin },
                    MarginPct = new { agg = marginPct },
                    PctOfGoal = new { agg = pctOfGoal },
                    Impressions = new { sum = impressions },
                    Clicks = new { sum = clicks },
                    TotalConv = new { sum = totalConv },
                    PostClickConv = new { sum = postClickConv },
                    PostViewConv = new { sum = postViewConv },
                    CTR = new { agg = ctr },
                    CPA = new { agg = cpa }
                };
                return aggs;
            }
            else
                return null;
        }
    }
}