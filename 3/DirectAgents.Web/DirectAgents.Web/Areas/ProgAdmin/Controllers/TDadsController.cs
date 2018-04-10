using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class TDadsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public TDadsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index(int? acctId)
        {
            var ads = cpProgRepo.TDads(acctId: acctId).OrderBy(a => a.Name).ThenBy(a => a.Id);
            Session["acctId"] = acctId.ToString();

            // Don't show images if not filtered (i.e. showing all creatives), unless requested explicitly
            ViewBag.ShowImages = (acctId.HasValue || (Request["images"] != null && Request["images"].ToUpper() == "TRUE"));
            return View(ads);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var ad = cpProgRepo.TDad(id);
            if (ad == null)
                return HttpNotFound();
            return View(ad);
        }
        
        [HttpPost]
        public ActionResult Edit(TDad ad)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SaveTDad(ad))
                    return RedirectToAction("Index", new { acctId = Session["acctId"] });
                ModelState.AddModelError("", "Creative could not be saved.");
            }
            cpProgRepo.FillExtended(ad);
            return View(ad);
        }
    }
}