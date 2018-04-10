using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class PlatformBudgetInfosController : DirectAgents.Web.Controllers.ControllerBase
    {
        public PlatformBudgetInfosController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult CreateNew(int campId, int platId, DateTime date)
        {
            var campaign = cpProgRepo.Campaign(campId);
            var platform = cpProgRepo.Platform(platId);
            if (campaign == null || platform == null)
                return HttpNotFound();
            BudgetInfoVals defaultBudgetInfo = campaign.BudgetInfoFor(date);
            if (defaultBudgetInfo == null)
                defaultBudgetInfo = campaign.DefaultBudgetInfo;
            var prevMonthPBI = cpProgRepo.PlatformBudgetInfo(campId, platId, date.AddMonths(-1));

            var pbi = new PlatformBudgetInfo(campId, platId, date);
            pbi.MediaSpend = (prevMonthPBI != null ? prevMonthPBI.MediaSpend : 0);
            pbi.MgmtFeePct = (prevMonthPBI != null ? prevMonthPBI.MgmtFeePct : defaultBudgetInfo.MgmtFeePct);
            pbi.MarginPct = (prevMonthPBI != null ? prevMonthPBI.MarginPct : defaultBudgetInfo.MarginPct);

            cpProgRepo.AddPlatformBudgetInfo(pbi);
            return RedirectToAction("Edit", "BudgetInfos", new { campId = campId, date = date.ToShortDateString() });
        }

        [HttpGet]
        public ActionResult Edit(int campId, int platId, DateTime date)
        {
            var pbi = cpProgRepo.PlatformBudgetInfo(campId, platId, date);
            if (pbi == null)
                return HttpNotFound();
            return View(pbi);
        }
        [HttpPost]
        public ActionResult Edit(PlatformBudgetInfo pbi)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SavePlatformBudgetInfo(pbi))
                    return RedirectToAction("Edit", "BudgetInfos", new { campId = pbi.CampaignId, date = pbi.Date.ToShortDateString() });
                ModelState.AddModelError("", "PlatformBudgetInfo could not be saved.");
            }
            cpProgRepo.FillExtended(pbi);
            return View(pbi);
        }

        public ActionResult Delete(int campId, int platId, DateTime date)
        {
            bool success = cpProgRepo.DeletePlatformBudgetInfo(campId, platId, date);
            if (success)
                return RedirectToAction("Edit", "BudgetInfos", new { campId = campId, date = date.ToShortDateString() });
            else
                return HttpNotFound();
        }
    }
}