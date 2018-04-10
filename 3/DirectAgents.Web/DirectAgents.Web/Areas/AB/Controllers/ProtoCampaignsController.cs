using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class ProtoCampaignsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ProtoCampaignsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index(int? clientId, int? accountId)
        {
            var campaigns = abRepo.ProtoCampaigns(clientId: clientId, clientAccountId: accountId);
            return View(campaigns);
        }

        public ActionResult New(int accountId)
        {
            var clientAccount = abRepo.ClientAccount(accountId);
            if (clientAccount == null)
                return HttpNotFound();

            var camp = new ProtoCampaign
            {
                Name = "zNew"
            };
            clientAccount.ProtoCampaigns.Add(camp);
            abRepo.SaveChanges();

            return RedirectToAction("Index", new { accountId = accountId });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var camp = abRepo.ProtoCampaign(id);
            if (camp == null)
                return HttpNotFound();
            return View(camp);
        }
        [HttpPost]
        public ActionResult Edit(ProtoCampaign camp)
        {
            if (ModelState.IsValid)
            {
                if (abRepo.SaveProtoCampaign(camp))
                    return RedirectToAction("Index", new { accountId = camp.ClientAccountId });
                ModelState.AddModelError("", "ProtoCampaign could not be saved.");
            }
            abRepo.FillExtended(camp);
            return View(camp);
        }
	}
}