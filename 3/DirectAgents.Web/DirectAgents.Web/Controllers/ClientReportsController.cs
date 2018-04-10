using System;
using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;

namespace DirectAgents.Web.Controllers
{
    public class ClientReportsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ClientReportsController(ICPSearchRepository cpSearchRepository)
        {
            this.cpSearchRepo = cpSearchRepository;
        }

        public ActionResult Index()
        {
            var reps = cpSearchRepo.ClientReports();
            return View(reps);
        }
    }
}