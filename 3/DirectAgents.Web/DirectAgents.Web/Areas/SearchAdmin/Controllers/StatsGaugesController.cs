using System;
using System.Linq;
using System.Web.Mvc;
using CakeExtracter.Commands;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities.CPSearch;

namespace DirectAgents.Web.Areas.SearchAdmin.Controllers
{
    public class StatsGaugesController : DirectAgents.Web.Controllers.ControllerBase
    {
        public StatsGaugesController(ICPSearchRepository cpSearchRepository)
        {
            this.cpSearchRepo = cpSearchRepository;
        }

        public ActionResult Index()
        {
            var gaugesByChannel = cpSearchRepo.StatsGaugesByChannel();
            return View(gaugesByChannel);
        }
    }
}