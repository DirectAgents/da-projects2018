using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CakeExtracter.Commands;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.Cake;
using DirectAgents.Web.Models;

namespace DirectAgents.Web.Controllers
{
    public class BudgetsController : ControllerBase
    {
        public BudgetsController(IMainRepository mainRepository, ISecurityRepository securityRepository)
        {
            this.daRepo = mainRepository;
            this.securityRepo = securityRepository;
        }

        // ---

        private IEnumerable<Contact> GetAccountManagers()
        {
            IEnumerable<Contact> accountManagers = daRepo.GetAccountManagers().ToList();
            if (!securityRepo.IsAdmin(User))
            {
                var amNames = securityRepo.AccountManagersForUser(User, true);
                accountManagers = accountManagers.Where(am => amNames.Contains(am.FullName));
            }
            return accountManagers;
        }
        private IEnumerable<int> GetAccountManagerIds()
        {
            return GetAccountManagers().Select(am => am.ContactId);
        }

        // ---

        public ActionResult Admin()
        {
            return View();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Start");
        }

        // ---

        public ActionResult Start()
        {
            var accountManagers = GetAccountManagers();
            return View(accountManagers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName));
        }

        //unused?
        public ActionResult AccountManagers()
        {
            var accountManagers = GetAccountManagers();
            return View(accountManagers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName));
        }

        public ActionResult Advertisers(int? am, bool showAll = false)
        {
            bool? withBudgetedOffers = showAll ? null : (bool?)true;
            var advertisers = daRepo.GetAdvertisers(am, withBudgetedOffers).OrderBy(a => a.AdvertiserName);
            var model = new BudgetsVM
            {
                Advertisers = advertisers,
                ShowAll = showAll
            };
            if (am.HasValue)
                model.AccountManager = daRepo.GetContact(am.Value);

            return View(model);
        }

        public ActionResult AdvertiserRow(int advId)
        {
            var advertiser = daRepo.GetAdvertiser(advId);
            if (advertiser == null)
                return null;
            return PartialView(advertiser);
        }

        public ActionResult Offers(string sort, bool? desc, int? am, int? advId, bool? withBudget, int? minPercent, bool includeInactive = false)
        {
            var model = new BudgetsVM()
            {
                Sort = (sort == null ? null : sort.ToLower()),
                SortDesc = desc.HasValue && desc.Value,
                AcctMgrId = am,
                AdvId = advId,
                WithBudget = withBudget,
                MinPercent = minPercent,
                IncludeInactive = includeInactive
            };

            bool includeExtended = false;
            var offers = daRepo.GetOffers(includeExtended, am, advId, withBudget, includeInactive);
            foreach (var offer in offers)
            {
                daRepo.FillOfferBudgetStats(offer);
            }
            IEnumerable<Offer> offersList = offers.ToList();
            if (minPercent.HasValue)
            {
                decimal minPercentDec = minPercent.Value / 100m;
                offersList = offersList.Where(o => o.BudgetUsedPercent.HasValue && o.BudgetUsedPercent.Value >= minPercentDec);
            }
            switch (model.Sort)
            {
                case "spentpct":
                    offersList = model.SortDesc ? offersList.OrderByDescending(o => o.BudgetUsedPercent)
                                                : offersList.OrderBy(o => o.BudgetUsedPercent);
                    break;
                default:
                    offersList = model.SortDesc ? offersList.OrderByDescending(o => o.Advertiser.AdvertiserName).ThenByDescending(o => o.OfferId)
                                                : offersList.OrderBy(o => o.Advertiser.AdvertiserName).ThenBy(o => o.OfferId);
                    break;
            } //TODO: rework the ToList's

            model.Offers = offersList;
            return View(model);
            //TODO: verify offer properties are lazy-loaded
        }

        public ActionResult OfferRow(int offerId, bool? editMode)
        {
            var offer = daRepo.GetOffer(offerId, false, true);
            if (offer == null)
                return null;

            ViewBag.EditMode = editMode;
            return PartialView(offer);
        }

        [HttpPost]
        public JsonResult SaveOfferRow(Offer inOffer)
        {
            bool saved = false;
            if (ModelState.IsValid)
                saved = SaveOfferBudget(inOffer);
            return Json(saved);
        }

        public ActionResult Show(int offerId)
        {
            var offer = daRepo.GetOffer(offerId, false, false);
            if (offer == null)
                return Content("Offer not found");

            var ods = daRepo.GetOfferDailySummariesForBudget(offer);
            var model = new OfferVM(offer, ods);
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int offerId)
        {
            var offer = daRepo.GetOffer(offerId, false, false);
            if (offer == null)
                return Content("Offer not found");

            return View(offer);
        }

        [HttpPost]
        public ActionResult Edit(Offer inOffer)
        {
            if (ModelState.IsValid)
            {
                bool saved = SaveOfferBudget(inOffer);
                if (saved)
                    return RedirectToAction("Show", new { offerId = inOffer.OfferId });

                ModelState.AddModelError("", "Offer Budget could not be saved");
            }
            var offer = daRepo.GetOffer(inOffer.OfferId, false, false);
            if (offer != null)
                return View(offer);
            else
                return View(inOffer);
        }

        private bool SaveOfferBudget(Offer inOffer)
        {
            var offer = daRepo.GetOffer(inOffer.OfferId, false, false);
            if (offer == null)
                return false;

            offer.Budget = inOffer.Budget;
            offer.BudgetStart = inOffer.BudgetStart;
            offer.BudgetEnd = inOffer.BudgetEnd;
            daRepo.SaveChanges();
            return true;
            //TODO: catch exceptions and return false?
        }

        // --- Cake Synching ---

        public ActionResult SynchAdvertisers()
        {
            DASynchAdvertisers.RunStatic(0, true, false);
            return RedirectToAction("Advertisers");
        }

        public ActionResult SynchBudgetStats()
        {
            DASynchOfferBudgetStats.RunStatic(null);
            return Content("Synch Stats complete");
        }

        public ActionResult SynchStats(int offerId)
        {
            var result = DASynchOfferBudgetStats.RunStatic(offerId);

            if (Request.IsAjaxRequest())
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return RedirectToAction("Show", new { offerId = offerId });
        }

        public ActionResult SynchOffers(int advId)
        {
            DASynchOffers.RunStatic(advId, false);
            return Content("Synch Offers complete");
        }

        // ---

        // Set budget to 0 (only if it is doesn't exist)
        public JsonResult Initialize(int offerId)
        {
            var offer = daRepo.GetOffer(offerId, false, false);
            if (offer != null && !offer.HasBudget)
            {
                offer.Budget = 0;
                daRepo.SaveChanges();
            }
            return null;
        }

        public ActionResult Test()
        {
            StringBuilder text = new StringBuilder();
            var affIds = new List<int>();
            var groups = securityRepo.GroupsForUser(User);
            foreach (var g in groups)
            {
                text.Append("Group: " + g.WindowsIdentity + "<br>");
            }
            return Content(text.ToString());
        }

    }
}
