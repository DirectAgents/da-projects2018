using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Web.Areas.ProgAdmin.Models;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class ExtraItemsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ExtraItemsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index(int? campId, DateTime? month)
        {
            Campaign campaign = null;
            if (campId.HasValue)
            {
                campaign = cpProgRepo.Campaign(campId.Value);
                if (campaign == null)
                    return HttpNotFound();
            }
            DateTime? startDate = null, endDate = null;
            if (month.HasValue)
            {
                startDate = SetChooseMonthViewData_NonCookie("RT", month);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }
            var items = cpProgRepo.ExtraItems(startDate, endDate, campId: campId);

            var model = new ExtraItemsVM
            {
                Campaign = campaign,
                Month = month.HasValue ? startDate : null,
                Items = items.OrderBy(i => i.Date).ThenBy(i => i.Id)
            };
            Session["campId"] = campId.ToString();
            Session["month"] = (month.HasValue ? month.Value.ToShortDateString() : ""); //TODO: set to startDate?
            return View(model);
        }

        public ActionResult CreateNew(int campId, DateTime? date)
        {
            var platformTD = cpProgRepo.Platform(Platform.Code_DATradingDesk);
            if (platformTD == null)
                return HttpNotFound();
            if (!date.HasValue)
                date = DateTime.Today.FirstDayOfMonth();

            var extraItem = new ExtraItem
            {
                Date = date.Value,
                CampaignId = campId,
                PlatformId = platformTD.Id,
                //Description =
            };
            if (cpProgRepo.AddExtraItem(extraItem))
                return RedirectToAction("Index", new { campId = Session["campId"], month = Session["month"] });
            else
                return Content("Error creating Extra Item");
        }
        public ActionResult Delete(int id)
        {
            cpProgRepo.DeleteExtraItem(id);
            return RedirectToAction("Index", new { campId = Session["campId"], month = Session["month"] });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var item = cpProgRepo.ExtraItem(id);
            if (item == null)
                return HttpNotFound();
            SetupForEdit();
            return View(item);
        }
        [HttpPost]
        public ActionResult Edit(ExtraItem item)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SaveExtraItem(item))
                    return RedirectToAction("Index", new { campId = Session["campId"], month = Session["month"] });
                ModelState.AddModelError("", "ExtraItem could not be saved.");
            }
            cpProgRepo.FillExtended(item);
            SetupForEdit();
            return View(item);
        }
        private void SetupForEdit()
        {
            ViewBag.Platforms = cpProgRepo.Platforms().OrderBy(p => p.Name).ToList();
            //TODO: fill campaign name? FillExtended?
        }
	}
}