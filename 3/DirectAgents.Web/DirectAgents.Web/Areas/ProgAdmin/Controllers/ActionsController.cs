using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class ActionsController : DirectAgents.Web.Controllers.ControllerBase
    {
        public ActionsController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Types()
        {
            var actionTypes = cpProgRepo.ActionTypes();
            return View(actionTypes);
        }
	}
}