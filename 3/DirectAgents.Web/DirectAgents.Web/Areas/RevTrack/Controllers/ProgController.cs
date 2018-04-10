using System.Web.Mvc;
using DirectAgents.Domain.Abstract;

namespace DirectAgents.Web.Areas.RevTrack.Controllers
{
    public class ProgController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ProgController(IRevTrackRepository rtRepository)
        {
            this.rtRepo = rtRepository;
        }

        public ActionResult Clients()
        {
            var progClients = rtRepo.ProgClients();
            return View(progClients);
        }
	}
}