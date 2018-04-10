using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class AdvertisersController : DirectAgents.Web.Controllers.ControllerBase
    {
        public AdvertisersController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index()
        {
            var advertisers = cpProgRepo.Advertisers() //TODO? don't load images? (also check ClientPortal.Web)
                .OrderBy(a => a.Name);

            return View(advertisers);
        }

        public ActionResult IndexFees(DateTime? month, bool activeLastMonth = false)
        {
            DateTime currMonth = SetChooseMonthViewData_NonCookie("RT", month);
            //DateTime currMonth = SetChooseMonthViewData("RT");

            var whichMonthToCheck = activeLastMonth ? currMonth.AddMonths(-1) : currMonth;
            var advList = GetActiveAdvertisers(whichMonthToCheck, includeThoseWithInfos: true);

            ViewBag.CurrMonth = currMonth;
            ViewBag.ActiveLastMonth = activeLastMonth;
            return View(advList);
        }
        private List<Advertiser> GetActiveAdvertisers(DateTime? month, bool includeThoseWithInfos = false)
        {
            var activeAccountIds = cpProgRepo.ExtAccountIds_Active(month).ToArray();
            var dbAdvertisers = cpProgRepo.Advertisers(includePlatforms: true).OrderBy(a => a.Name);
            var advList = new List<Advertiser>();
            foreach (var dbAdv in dbAdvertisers)
            {
                if (dbAdv.Campaigns != null)
                {
                    //See if any of the advertiser's campaign's are active (i.e. have an account with DailySummaries) or have BudgetInfos...
                    bool useIt = false;
                    foreach (var camp in dbAdv.Campaigns)
                    {
                        int[] campAcctIds = (camp.ExtAccounts != null) ? camp.ExtAccounts.Select(x => x.Id).ToArray() : new int[] { };
                        if (campAcctIds.Any(x => activeAccountIds.Contains(x)))
                            useIt = true;
                        else if (includeThoseWithInfos)
                        {
                            if ((camp.BudgetInfos != null && camp.BudgetInfos.Any(x => x.Date == month)) ||
                                 (camp.PlatformBudgetInfos != null && camp.PlatformBudgetInfos.Any(x => x.Date == month)))
                                useIt = true;
                        }
                        if (useIt)
                            break;
                    }
                    if (useIt)
                        advList.Add(dbAdv);
                }
            }
            return advList;
        }

        // Copy the BudgetInfos & PlatformBudgets infos to the specified month from the month prior.
        // Do this for active campaigns/platforms. (can specify activeLastMonth)
        public ActionResult CopyInfos(DateTime month, bool activeLastMonth = false, bool overwrite = false)
        {
            var prevMonth = month.AddMonths(-1);
            var whichMonthToCheck = activeLastMonth ? prevMonth : month;
            var campaigns = cpProgRepo.CampaignsActive(monthStart: whichMonthToCheck);
            foreach (var camp in campaigns)
            {
                var prevBI = camp.BudgetInfoFor(prevMonth);
                if (prevBI != null)
                {
                    var existingBI = camp.BudgetInfoFor(month);
                    if (existingBI == null)
                    {
                        var newBI = new BudgetInfo(camp.Id, month, valuesToSet: prevBI);
                        cpProgRepo.AddBudgetInfo(newBI, saveChanges: false);
                    }
                    else if (overwrite)
                    {
                        existingBI.SetFrom(prevBI);
                    }
                }
                var pbis = camp.PlatformBudgetInfosFor(prevMonth).ToList();
                foreach (var prevPBI in pbis)
                {
                    var existingPBI = camp.PlatformBudgetInfoFor(month, prevPBI.PlatformId, false);
                    if (existingPBI == null)
                    {
                        var newPBI = new PlatformBudgetInfo(camp.Id, prevPBI.PlatformId, month, valuesToSet: prevPBI);
                        cpProgRepo.AddPlatformBudgetInfo(newPBI, saveChanges: false);
                    }
                    else if (overwrite)
                    {
                        existingPBI.SetFrom(prevPBI);
                    }
                }
            }
            cpProgRepo.SaveChanges();

            return RedirectToAction("IndexFees", new { month = month.ToShortDateString(), activeLastMonth = activeLastMonth });
        }

        public ActionResult CreateBaseFees(DateTime month)
        {
            var tdPlatform = cpProgRepo.Platform(Platform.Code_DATradingDesk);
            cpProgRepo.CreateBaseFees(month, tdPlatform.Id);
            return RedirectToAction("Index", "ExtraItems", new { month = month.ToShortDateString() });
        }

        public ActionResult CreateNew()
        {
            var advertiser = new Advertiser
            {
                Name = "zNew"
            };
            if (cpProgRepo.AddAdvertiser(advertiser))
                return RedirectToAction("Index");
            else
                return Content("Error creating Advertiser");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var advertiser = cpProgRepo.Advertiser(id);
            if (advertiser == null)
                return HttpNotFound();
            SetupForEdit();
            return View(advertiser);
        }
        [HttpPost]
        public ActionResult Edit(Advertiser adv)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SaveAdvertiser(adv, includeLogo: false))
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Campaign could not be saved.");
            }
            //fillextended?
            SetupForEdit();
            return View(adv);
        }
        private void SetupForEdit()
        {
            ViewBag.Employees = cpProgRepo.Employees().OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();
        }

        public FileResult Logo(int id)
        {
            var advertiser = cpProgRepo.Advertiser(id);
            if (advertiser == null)
                return null;
            WebImage logo = new WebImage(advertiser.Logo);
            return File(logo.GetBytes(), "image/" + logo.ImageFormat, logo.FileName);
        }
        public ActionResult EditLogo(int id)
        {
            var advertiser = cpProgRepo.Advertiser(id);
            if (advertiser == null)
                return HttpNotFound();
            return View(advertiser);
        }
        [HttpPost]
        public ActionResult UploadLogo(int id)
        {
            var advertiser = cpProgRepo.Advertiser(id);
            if (advertiser == null)
                return null;

            WebImage logo = WebImage.GetImageFromRequest();
            byte[] imageBytes = logo.GetBytes();

            advertiser.Logo = imageBytes;
            cpProgRepo.SaveChanges();

            return null;
        }
    }
}