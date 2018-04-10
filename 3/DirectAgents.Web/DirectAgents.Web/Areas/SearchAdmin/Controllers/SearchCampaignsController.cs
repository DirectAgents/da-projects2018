using System;
using System.Linq;
using System.Web.Mvc;
using CakeExtracter.Commands;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPSearch;

namespace DirectAgents.Web.Areas.SearchAdmin.Controllers
{
    public class SearchCampaignsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public SearchCampaignsController(ICPSearchRepository cpSearchRepository)
        {
            this.cpSearchRepo = cpSearchRepository;
        }

        public ActionResult Index(int? spId, int? saId)
        {
            var searchCamps = cpSearchRepo.SearchCampaigns(spId: spId, searchAccountId: saId);
            return View(searchCamps.OrderBy(x => x.SearchAccountId).ThenBy(x => x.SearchCampaignName));
        }

        public ActionResult IndexGauge(int? spId, int? saId, string channel)
        {
            var searchCamps = cpSearchRepo.SearchCampaigns(spId: spId, searchAccountId: saId, channel: channel, includeGauges: true);
            return View(searchCamps.OrderBy(x => x.SearchAccountId).ThenBy(x => x.SearchCampaignName));
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var sc = cpSearchRepo.GetSearchCampaign(id);
            if (sc == null)
                return HttpNotFound();
            return View(sc);
        }
        [HttpPost]
        public ActionResult Edit(SearchCampaign inSC)
        {
            var sc = cpSearchRepo.GetSearchCampaign(inSC.SearchCampaignId);
            if (sc == null)
                return HttpNotFound();
            sc.LCcmpid = inSC.LCcmpid;
            cpSearchRepo.SaveChanges();

            return RedirectToAction("IndexGauge", new { spId = sc.SearchAccount.SearchProfileId });
        }

    }
}