using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Web.Areas.ProgAdmin.Models;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class CampaignsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public CampaignsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index(int? advId)
        {
            var campaigns = cpProgRepo.Campaigns(advId: advId)
                .OrderBy(c => c.Advertiser.Name).ThenBy(c => c.Name);

            Session["advId"] = advId.ToString();
            return View(campaigns);
        }

        public ActionResult Dashboard(bool all = false)
        {
            DateTime currMonth = SetChooseMonthViewData("RT");
            IEnumerable<Campaign> campaigns;
            if (all)
                campaigns = cpProgRepo.Campaigns();
            else
                campaigns = cpProgRepo.CampaignsActive(currMonth);

            var model = new DashboardVM
            {
                Month = currMonth,
                Campaigns = campaigns.OrderBy(c => c.Advertiser.Name).ThenBy(c => c.Name),
                ShowAll = all
            };
            return View(model);
        }

        public ActionResult Show(int id)
        {
            var campaign = cpProgRepo.Campaign(id);
            if (campaign == null)
                return HttpNotFound();

            return View(campaign);
        }

        public ActionResult CreateNew(int advId)
        {
            var adv = cpProgRepo.Advertiser(advId);
            if (adv == null)
                return HttpNotFound();

            // Make the first campaign for an advertiser have the same name as the advertiser
            string campName = adv.Name;
            if (adv.Campaigns != null && adv.Campaigns.Any(x => x.Name == campName))
                campName = "zNew";

            var campaign = new Campaign
            {
                AdvertiserId = advId,
                Name = campName,
                DefaultBudgetInfo = new BudgetInfoVals
                {
                    MgmtFeePct = 15,
                    MarginPct = 30
                }
            };
            if (cpProgRepo.AddCampaign(campaign))
                return RedirectToAction("Index", new { advId = Session["advId"] });
            else
                return Content("Error creating Campaign");
        }
        public ActionResult Delete(int id)
        {
            cpProgRepo.DeleteCampaign(id);
            return RedirectToAction("Index", new { advId = Session["advId"] });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var campaign = cpProgRepo.Campaign(id);
            if (campaign == null)
                return HttpNotFound();
            SetupForEdit(id);
            return View(campaign);
        }
        [HttpPost]
        public ActionResult Edit(Campaign camp)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SaveCampaign(camp))
                    return RedirectToAction("Edit", new { id = camp.Id });
                    //return RedirectToAction("Index", new { advId = Session["advId"] });
                ModelState.AddModelError("", "Campaign could not be saved.");
            }
            cpProgRepo.FillExtended(camp);
            SetupForEdit(camp.Id);
            return View(camp);
        }
        private void SetupForEdit(int campId)
        {
            ViewBag.ExtAccounts = cpProgRepo.ExtAccountsNotInCampaign(campId)
                .OrderBy(a => a.Platform.Name).ThenBy(a => a.Name).ThenBy(a => a.ExternalId);
        }

        public ActionResult AddAccount(int id, int acctId)
        {
            cpProgRepo.AddExtAccountToCampaign(id, acctId);
            return RedirectToAction("Edit", new { id = id });
        }
        public ActionResult RemoveAccount(int id, int acctId)
        {
            cpProgRepo.RemoveExtAccountFromCampaign(id, acctId);
            return RedirectToAction("Edit", new { id = id });
        }
    }
}