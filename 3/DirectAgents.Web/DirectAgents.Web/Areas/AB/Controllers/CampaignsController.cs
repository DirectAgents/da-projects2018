using System.Web.Mvc;
using DirectAgents.Domain.Abstract;

namespace DirectAgents.Web.Areas.AB.Controllers
{
    public class CampaignsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public CampaignsController(IABRepository abRepository)
        {
            this.abRepo = abRepository;
        }

        public ActionResult Index(int clientId)
        {
            var client = abRepo.Client(clientId);
            if (client == null)
                return HttpNotFound();

            client.CampaignWraps = abRepo.CampaignWraps(clientId: clientId);
            return View(client);
        }

        public ActionResult BucketList(int acctId)
        {
            var clientAccount = abRepo.ClientAccount(acctId);
            if (clientAccount == null)
                return HttpNotFound();

            var campaignWraps = abRepo.CampaignWraps(clientId: clientAccount.ClientId);
            ViewBag.CampaignWraps = campaignWraps;

            return View(clientAccount);
        }
	}
}