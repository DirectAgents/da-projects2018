using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.Cake;
using DirectAgents.Web.Models;

namespace DirectAgents.Web.Controllers
{
    public class StatsController : ControllerBase
    {
        public StatsController(IMainRepository mainRepository, ITDRepository tdRepository)
        {
            this.daRepo = mainRepository;
            this.tdRepo = tdRepository;
        }

        // ---

        public ActionResult Links()
        {
            return View();
        }

        public ActionResult Test(bool p = false)
        {
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var stats = daRepo.GetStatsSummary(null, startDate, null);
            var json = Json(stats, JsonRequestBehavior.AllowGet);
            if (p)
                return json.ToJsonp();
            else
                return json;
        }

        // Set numdays=-1 for MTD, 0 for today (so far)
        // Set take=0 for all
        public ActionResult CakeAdv(string sort, int numdays = -1, int take = 0, string jsoncallback = null)
        {
            var advStats = GetAdvStats(sort, numdays, take);
            if (!string.IsNullOrWhiteSpace(jsoncallback))
            {
                var json = Json(advStats, JsonRequestBehavior.AllowGet);
                return json.ToJsonp();
            }
            else
            {
                var model = new ScreensVM { AdvStats = advStats };
                return View(model);
            }
        }
        private IEnumerable<StatsSummary> GetAdvStats(string sort, int numdays = -1, int take = 0)
        {
            var today = DateTime.Today;
            DateTime startDate;
            if (numdays < 1) // MTD
                startDate = new DateTime(today.Year, today.Month, 1);
            else
                startDate = today.AddDays(-numdays);

            var ods = daRepo.GetOfferDailySummaries(null, startDate);
            var offerSummaries = ods.GroupBy(o => o.OfferId).Select(g =>
                new StatsSummary
                {
                    Id = g.Key,
                    Views = g.Sum(o => o.Views),
                    Clicks = g.Sum(o => o.Clicks),
                    Conversions = g.Sum(o => o.Conversions),
                    Paid = g.Sum(o => o.Paid),
                    Sellable = g.Sum(o => o.Sellable),
                    Revenue = g.Sum(o => o.Revenue),
                    Cost = g.Sum(o => o.Cost)
                }).ToList();

            IEnumerable<StatsSummary> advStats = new List<StatsSummary>();
            var advertisers = daRepo.GetAdvertisers().ToList();
            foreach (var adv in advertisers)
            {
                adv.OfferIds = daRepo.OfferIds(advId: adv.AdvertiserId);

                if (adv.OfferIds.Any())
                {
                    var advSummaries = offerSummaries.Where(os => adv.OfferIds.Contains(os.Id.Value));
                    if (advSummaries.Any())
                    {
                        var stats = new StatsSummary
                        {
                            Name = adv.AdvertiserName,
                            Views = advSummaries.Sum(ds => ds.Views),
                            Clicks = advSummaries.Sum(ds => ds.Clicks),
                            Conversions = advSummaries.Sum(ds => ds.Conversions),
                            Paid = advSummaries.Sum(ds => ds.Paid),
                            Sellable = advSummaries.Sum(ds => ds.Sellable),
                            Revenue = advSummaries.Sum(ds => ds.Revenue),
                            Cost = advSummaries.Sum(ds => ds.Cost)
                        };
                        ((List<StatsSummary>)advStats).Add(stats);
                    }
                }
            }
            if (sort == "margin")
                advStats = advStats.OrderByDescending(a => a.Margin);
            else if (sort == "cost")
                advStats = advStats.OrderByDescending(a => a.Cost);
            else // default: by revenue
                advStats = advStats.OrderByDescending(a => a.Revenue);

            if (take > 0)
                advStats = advStats.Take(take);

            return advStats;
        }

        public JsonResult CakeSummary(int numdays = -1)
        {
            var today = DateTime.Today;
            DateTime startDate;
            if (numdays < 1) // default: MTD
                startDate = new DateTime(today.Year, today.Month, 1);
            else
                startDate = today.AddDays(-numdays);
            var stats = daRepo.GetStatsSummary(null, startDate, null);
            //Note: Starting 7/1/15, includes clicks, conversions and sellable from offerid -1 (Global Redirect) and clicks from offerid -2 (404)

            var json = Json(stats, JsonRequestBehavior.AllowGet);
            return json.ToJsonp();
        }
    }
}